using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class ValueTypeDumpObject : DumpObject
{
    private readonly ClrValueType _clrValueType;
    private readonly Dump _owner;

    public ValueTypeDumpObject(ClrValueType clrValueType, Dump owner)
    {
        _clrValueType = clrValueType;
        _owner = owner;
    }

    public override DumpObject GetField(string name)
    {
        return _owner.Factory.GetFieldDumpObject(_clrValueType, name, this);
    }
    
    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        if (TryRenderValue(out var value))
        {
            sb.Append(value);
            return;
        }
        
        sb.Append(_clrValueType.Type.Name)
            .Append(" @ ")
            .Append($"0x{_clrValueType.Address:X16}");
        
        if (depth > 0)
        {
            foreach (var fieldType in _clrValueType.Type.Fields)
            {
                var field = GetField(fieldType.Name);
                
                sb.AppendLine()
                    .Append(' ', (indention + 1)*2)
                    .Append(fieldType.Name)
                    .Append(": ");
                
                field.BuildString(sb, depth - 1, arrayItems, indention + 1);
            }
        }
    }

    private bool TryGetValueReader([NotNullWhen(true)] out IUnmanagedValueReader? valueReader)
    {
        if (TryGetField0(out var field0))
        {
            valueReader = new ValueTypeValueReader(_clrValueType, field0.Name);
            return true;
        }
        valueReader = null;
        return false;
    }

    private bool TryGetField0([NotNullWhen(true)] out ClrInstanceField? field0)
    {
        // This is a bit of a hack, but since ClrValueType does not have an API to read its value into an unmanaged
        // struct, we just find a field with offset zero, and read from that field (address will be same)
        foreach (var field in _clrValueType.Type.Fields)
        {
            if (field.Offset == 0)
            {
                field0 = field;
                return true;
            }
        }
        field0 = null;
        return false;
    }

    public override ulong Address => _clrValueType.Address;
    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        if (_owner.TryGetValueRender(_clrValueType.Type.Name, out var renderer)
            && TryGetValueReader(out var valueReader))
        {
            value = renderer(valueReader);
            return true;
        }

        value = null;
        return false;
    }

    public override bool IsArray() => false;
    public override IEnumerable<FieldInfo> GetFields()
    {
        foreach (var field in _clrValueType.Type.Fields)
        {
            yield return new FieldInfo(field.Name);
        }
    }

    public override bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = null;
        return false;
    }

    public override DumpObjectValue<T> ReadAs<T>()
    {
        if (TryGetField0(out var field0))
        {
            return DumpObjectValue<T>.Create(_clrValueType.ReadField<T>(field0.Name));
        }
        
        return DumpObjectValue<T>.CreateFailedToFindField0(_clrValueType.Type.Name);
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.CreateFailedUnsupportedTypeForString(_clrValueType.Type.Name);
    }

    private class ValueTypeValueReader : IUnmanagedValueReader
    {
        private readonly ClrValueType _clrValueType;
        private readonly string _field0Name;

        public ValueTypeValueReader(ClrValueType clrValueType, string field0Name)
        {
            _clrValueType = clrValueType;
            _field0Name = field0Name;
        }


        public T Read<T>() where T : unmanaged
        {
            return _clrValueType.ReadField<T>(_field0Name);
        }
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}