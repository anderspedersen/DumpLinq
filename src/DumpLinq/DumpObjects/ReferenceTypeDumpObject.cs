using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class ReferenceTypeDumpObject : DumpObject
{
    private readonly ClrObject _clrObject;
    private readonly Dump _owner;

    public ReferenceTypeDumpObject(ClrObject clrObject, Dump owner)
    {
        _clrObject = clrObject;
        _owner = owner;
    }

    public override DumpObject GetField(string name)
    {
        return _owner.Factory.GetFieldDumpObject(_clrObject, name, this);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append(_clrObject.Type.Name)
            .Append(" @ ")
            .Append($"0x{_clrObject.Address:X16}");

        if (depth > 0)
        {
            foreach (var fieldType in _clrObject.Type.Fields)
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

    public override ulong Address => _clrObject.Address;
    public override DumpObjectValue<T> ReadAs<T>()
    {
        return DumpObjectValue<T>.CreateFailedUnsupportedTypeForValueType(_clrObject.Type.Name);
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.CreateFailedUnsupportedTypeForString(_clrObject.Type.Name);
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}