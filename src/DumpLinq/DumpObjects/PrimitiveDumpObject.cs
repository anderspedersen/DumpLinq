using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace DumpLinq.DumpObjects;

internal class PrimitiveDumpObject<T> : DumpObject where T : unmanaged
{
    private T _value;
    private readonly ulong _address;

    public PrimitiveDumpObject(T value, ulong address)
    {
        _value = value;
        _address = address;
    }

    public override DumpObject GetField(string name)
    {
        return new NotFoundFieldDumpObject(name, this);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append(_value);
    }

    public override ulong Address => _address;

    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        value = _value.ToString()!;
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

    public override DumpObjectValue<TReturn> ReadAs<TReturn>()
    {
        if (Unsafe.SizeOf<TReturn>() != Unsafe.SizeOf<T>())
        {
            return DumpObjectValue<TReturn>.CreateFailedSizesNotMatching(typeof(T).Name, Unsafe.SizeOf<T>());
        }
        return DumpObjectValue<TReturn>.Create(Unsafe.As<T, TReturn>(ref _value));
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.CreateFailedUnsupportedTypeForString(typeof(T).Name);
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}