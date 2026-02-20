using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class StringDumpObject : DumpObject
{
    private readonly ClrObject _clrObject;

    public StringDumpObject(ClrObject clrObject)
    {
        _clrObject = clrObject;
    }

    public override DumpObject GetField(string name)
    {
        return new NotFoundFieldDumpObject(name, this);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        if (indention == 0)
        {
            sb.Append(_clrObject.Type.Name)
                .Append(" @ ")
                .Append($"0x{_clrObject.Address:X16}")
                .Append(' ');
        }

        sb.Append('"')
            .Append(_clrObject.AsString())
            .Append('"');
    }

    public override ulong Address => _clrObject.Address;
    
    public override bool TryRenderValue(out string? value)
    {
        value = AsString();
        return true;
    }

    public override bool IsArray() => false;
    public override IEnumerable<FieldInfo> GetFields()
    {
        return Enumerable.Empty<FieldInfo>();
    }

    public override bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = null;
        return false;
    }

    public override DumpObjectValue<T> ReadAs<T>()
    {
        return DumpObjectValue<T>.CreateFailedUnsupportedTypeForValueType(_clrObject.Type.Name);
    }

    public override DumpObjectValue<string> AsString(int maxLength = 4096)
    {
        return DumpObjectValue<string>.Create(_clrObject.AsString(maxLength)!);
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}