using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class BoxedDumpObject : DumpObject
{
    private readonly ClrObject _clrObject;
    private readonly Dump _owner;

    public BoxedDumpObject(ClrObject clrObject, Dump owner)
    {
        _clrObject = clrObject;
        _owner = owner;
    }

    public override DumpObject GetField(string name)
    {
        return new ReferenceTypeDumpObject(_clrObject, _owner).GetField(name);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append(_clrObject.Type.Name)
            .Append(" @ ")
            .Append($"0x{_clrObject.Address:X16}")
            .Append(" (boxed)");

        if (_clrObject.Type.IsPrimitive)
        {
            sb.Append(": ").Append(DumpObjectFactory.GetPrimitive(0, _clrObject.Type.ElementType, new UnboxingUnmanagedValueReader(_clrObject)).ToString());
            return;
        }

        if (_owner.TryGetValueRender(_clrObject.Type.Name, out var renderer))
        {
            sb.Append(": ").Append(renderer(new UnboxingUnmanagedValueReader(_clrObject)));
            return;
        }

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
        if (Unsafe.SizeOf<T>() == (int) _clrObject.Size)
        {
            return DumpObjectValue<T>.Create(_clrObject.ReadBoxedValue<T>());
        }
        return DumpObjectValue<T>.CreateFailedSizesNotMatching(_clrObject.Type.Name, (int) _clrObject.Size);
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

internal class UnboxingUnmanagedValueReader : IUnmanagedValueReader
{
    private readonly ClrObject _clrObject;

    public UnboxingUnmanagedValueReader(ClrObject clrObject)
    {
        _clrObject = clrObject;
    }

    public T Read<T>() where T : unmanaged
    {
        return _clrObject.ReadBoxedValue<T>();
    }
}