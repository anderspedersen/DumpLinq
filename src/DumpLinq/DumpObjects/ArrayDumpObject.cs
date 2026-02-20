using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class ArrayDumpObject : DumpObject
{
    private readonly ClrArray _clrArray;
    private readonly Dump _owner;

    public ArrayDumpObject(ClrArray clrArray, Dump owner)
    {
        _clrArray = clrArray;
        _owner = owner;
    }

    public override DumpObject GetField(string name)
    {
        if (name == "Length")
        {
            return new PrimitiveDumpObject<int>(_clrArray.Length, _clrArray.Address);
        }

        return new NotFoundFieldDumpObject(name, this);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append(_clrArray.Type.Name)
            .Append(" @ ")
            .Append($"0x{_clrArray.Address:X16}");
        
        if (depth > 0)
        {
            var indices = InitializeIndices();
            for (int i = 0; i < arrayItems && TryGetNextIndex(indices); i++)
            {
                var item = GetItem(indices);
                
                sb.AppendLine()
                    .Append(' ', (indention + 1)*2)
                    .Append(FormatIndices(indices))
                    .Append(": ");
                
                item.BuildString(sb, depth - 1, arrayItems, indention + 1);
            }
        }
    }

    private static string? FormatIndices(int[] indices)
    {
        var returnString = "[" + indices[0];

        for (int i = 1; i < indices.Length; i++)
        {
            returnString += ", " + indices[i];
        }
        return returnString + "]";
    }

    public override ulong Address => _clrArray.Address;

    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    public override bool IsArray() => true;
    
    public override IEnumerable<FieldInfo> GetFields()
    {
        yield return new FieldInfo("Length");
    }

    public override bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = null;
        return false;
    }

    public override DumpObjectValue<T> ReadAs<T>()
    {
        return DumpObjectValue<T>.CreateFailedUnsupportedTypeForValueType(_clrArray.Type.Name);
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.CreateFailedUnsupportedTypeForString(_clrArray.Type.Name);
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        var indices = InitializeIndices();

        while (TryGetNextIndex(indices))
        {
            yield return GetItem(indices);
        } 
    }

    private int[] InitializeIndices()
    {
        var rank = _clrArray.Rank;
        var indices = new int[rank];
        
        indices[0] = _clrArray.GetLowerBound(0) - 1;
        
        for (int i = 1; i < rank; i++)
        {
            indices[i] = _clrArray.GetLowerBound(i);
        }

        return indices;
    }

    private DumpObject GetItem(int[] indices)
    {
        if (_clrArray.Type.ComponentType!.IsObjectReference)
        {
            var item = _clrArray.GetObjectValue(indices);
            if (item.IsNull)
            {
                return new NullDumpObject(FormatIndices(indices), this);
            }
                
            return _owner.Factory.CreateDumpObject(item);
        }
        
        if (_clrArray.Type.ComponentType!.IsEnum)
        {
            return DumpObjectFactory.GetEnum(_clrArray.GetStructValue(indices).Address, _clrArray.Type.ComponentType!.AsEnum(), new ArrayUnmanagedValueReader(_clrArray, indices));
        }
        
        if (_clrArray.Type.ComponentType!.IsPrimitive)
        {
            return DumpObjectFactory.GetPrimitive(_clrArray.GetStructValue(indices).Address, _clrArray.Type.ComponentType!.ElementType, new ArrayUnmanagedValueReader(_clrArray, indices));
        }

        if (_clrArray.Type.ComponentType!.IsValueType)
        {
            return new ValueTypeDumpObject(_clrArray.GetStructValue(indices), _owner);
        }

        if (_clrArray.Type.ComponentType!.ElementType is ClrElementType.Pointer or ClrElementType.FunctionPointer)
        {
            return new PointerDumpObject(_clrArray.GetValue<nuint>(indices), _clrArray.GetStructValue(indices).Address);
        }

        return new NotFoundFieldDumpObject(FormatIndices(indices), this);
    }

    private bool TryGetNextIndex(int[] indices)
    {
        for (int i = 0; i < _clrArray.Rank; i++)
        {
            indices[i]++;
            if (indices[i] <= _clrArray.GetUpperBound(i))
            {
                return true;
            }
            
            indices[i] = _clrArray.GetLowerBound(i);
        }
        return false;
    }
}

internal class ArrayUnmanagedValueReader : IUnmanagedValueReader
{
    private readonly ClrArray _clrArray;
    private readonly int[] _indeces;

    public ArrayUnmanagedValueReader(ClrArray clrArray, int[] indeces)
    {
        _clrArray = clrArray;
        _indeces = indeces;
    }

    public T Read<T>() where T : unmanaged
    {
        return _clrArray.GetValue<T>(_indeces);
    }
}