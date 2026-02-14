using DumpLinq.DumpObjects;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;

namespace DumpLinq;

internal class DumpObjectFactory
{
    private readonly Dump _dump;

    public DumpObjectFactory(Dump dump)
    {
        _dump = dump;
    }

    internal DumpObject CreateDumpObject(ClrObject clrObject)
    {
        if (clrObject.IsNull)
        {
            throw new ArgumentException(nameof(clrObject));
        }

        if (clrObject.IsBoxedValue)
        {
            return new BoxedDumpObject(clrObject, _dump);
        }

        if (clrObject.IsArray)
        {
            return new ArrayDumpObject(clrObject.AsArray(), _dump);
        }

        if (clrObject.Type!.IsString)
        {
            return new StringDumpObject(clrObject);
        }

        return new ReferenceTypeDumpObject(clrObject, _dump);
    }

    internal static DumpObject GetPrimitive(ulong address, ClrElementType clrType, IUnmanagedValueReader reader)
    {
        return clrType switch
        {
            ClrElementType.Boolean => new PrimitiveDumpObject<bool>(reader.Read<bool>(), address),
            ClrElementType.Char => new PrimitiveDumpObject<char>(reader.Read<char>(), address),
            ClrElementType.Int8 => new PrimitiveDumpObject<sbyte>(reader.Read<sbyte>(), address),
            ClrElementType.UInt8 => new PrimitiveDumpObject<byte>(reader.Read<byte>(), address),
            ClrElementType.Int16 => new PrimitiveDumpObject<short>(reader.Read<short>(), address),
            ClrElementType.UInt16 => new PrimitiveDumpObject<ushort>(reader.Read<ushort>(), address),
            ClrElementType.Int32 => new PrimitiveDumpObject<int>(reader.Read<int>(), address),
            ClrElementType.UInt32 => new PrimitiveDumpObject<uint>(reader.Read<uint>(), address),
            ClrElementType.Int64 => new PrimitiveDumpObject<long>(reader.Read<long>(), address),
            ClrElementType.UInt64 => new PrimitiveDumpObject<ulong>(reader.Read<ulong>(), address),
            ClrElementType.Float => new PrimitiveDumpObject<float>(reader.Read<float>(), address),
            ClrElementType.Double => new PrimitiveDumpObject<double>(reader.Read<double>(), address),
            ClrElementType.NativeInt => new PrimitiveDumpObject<nint>(reader.Read<nint>(), address),
            ClrElementType.NativeUInt => new PrimitiveDumpObject<nuint>(reader.Read<nuint>(), address),
            _ => throw new InvalidOperationException()
        };
    }

    internal static DumpObject GetEnum(ulong address, ClrEnum clrEnum, IUnmanagedValueReader reader)
    {
        return clrEnum.ElementType switch
        {
            ClrElementType.Int8 => new EnumDumpObject<sbyte>(reader.Read<sbyte>(), address, clrEnum),
            ClrElementType.UInt8 => new EnumDumpObject<byte>(reader.Read<byte>(), address, clrEnum),
            ClrElementType.Int16 => new EnumDumpObject<short>(reader.Read<short>(), address, clrEnum),
            ClrElementType.UInt16 => new EnumDumpObject<ushort>(reader.Read<ushort>(), address, clrEnum),
            ClrElementType.Int32 => new EnumDumpObject<int>(reader.Read<int>(), address, clrEnum),
            ClrElementType.UInt32 => new EnumDumpObject<uint>(reader.Read<uint>(), address, clrEnum),
            ClrElementType.Int64 => new EnumDumpObject<long>(reader.Read<long>(), address, clrEnum),
            ClrElementType.UInt64 => new EnumDumpObject<ulong>(reader.Read<ulong>(), address, clrEnum),
            _ => throw new InvalidOperationException()
        };
    }

    internal DumpObject GetFieldDumpObject<TParent>(TParent clrValue, string name, DumpObject parent) where TParent : IClrValue
    {
        try
        {
            var type = clrValue.Type!;
            var field = (ClrInstanceField?) type.GetFieldByName(name);

            if (field is not null)
            {
                if (field.IsObjectReference)
                {
                    var member = (ClrObject) clrValue.ReadObjectField(name);
                    if (member.IsNull)
                    {
                        return new NullDumpObject(name, parent);
                    }

                    return CreateDumpObject(member);
                }

                if (field.Type.IsEnum)
                {
                    return GetEnum(clrValue.Address + (ulong) field.Offset, field.Type.AsEnum(), new FieldUnmanagedValueReader<TParent>(clrValue, name));

                }

                if (field.IsPrimitive)
                {
                    return GetPrimitive(clrValue.Address + (ulong) field.Offset, field.ElementType, new FieldUnmanagedValueReader<TParent>(clrValue, name));
                }

                if (field.IsValueType)
                {
                    return new ValueTypeDumpObject((ClrValueType) clrValue.ReadValueTypeField(name), _dump);
                }

                if (field.ElementType is ClrElementType.Pointer or ClrElementType.FunctionPointer)
                {
                    return new PointerDumpObject(clrValue.ReadField<nuint>(name), clrValue.Address + (ulong) field.Offset);
                }
            }

            return new NotFoundFieldDumpObject(name, parent);
        }
        catch (Exception e)
        {
            return new UnknownErrorDumpObject(e.Message, name, parent);
        }
    }

    private class FieldUnmanagedValueReader<TParent> : IUnmanagedValueReader where TParent : IClrValue
    {
        private TParent _parent;
        private readonly string _name;

        public FieldUnmanagedValueReader(TParent parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        public T Read<T>() where T : unmanaged
        {
            return _parent.ReadField<T>(_name);
        }
    }
}