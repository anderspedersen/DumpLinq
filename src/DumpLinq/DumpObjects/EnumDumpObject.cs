using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq.DumpObjects;

internal class EnumDumpObject<T> : DumpObject where T : IEqualityOperators<T,T,bool>
{
    private T _value;
    private readonly ulong _address;
    private readonly ClrEnum _clrEnum;

    public EnumDumpObject(T value, ulong address, ClrEnum clrEnum)
    {
        
        _value = value;
        _address = address;
        _clrEnum = clrEnum;
    }

    public override DumpObject GetField(string name)
    {
        return new NotFoundFieldDumpObject(name, this);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append(GetStringValue());
    }

    public override ulong Address => _address;

    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        value = GetStringValue();
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
        if (Unsafe.SizeOf<TReturn>() == Unsafe.SizeOf<T>())
        {
            return DumpObjectValue<TReturn>.Create(Unsafe.As<T, TReturn>(ref _value));
        }
        return DumpObjectValue<TReturn>.CreateFailedSizesNotMatching(typeof(T).Name, Unsafe.SizeOf<T>());
    }

    private string GetStringValue()
    {
        foreach (var enumValue in  _clrEnum.EnumerateValues())
        {
            if ((T) enumValue.Value == _value)
            {
                return enumValue.Name;
            }
        }

        return _value.ToString();
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.Create(GetStringValue());
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}