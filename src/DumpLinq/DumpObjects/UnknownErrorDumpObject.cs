using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DumpLinq.DumpObjects;

internal class UnknownErrorDumpObject : DumpObject
{
    private readonly string _error;

    public UnknownErrorDumpObject(string error, string name, DumpObject parent)
    {
        _error = error;
    }

    public override DumpObject GetField(string name)
    {
        return this;
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append("Unknow error: ")
            .Append(_error);
    }

    public override ulong Address => ulong.MaxValue;
    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    public override bool IsArray() => false;
    public override IEnumerable<FieldInfo> GetFields()
    {
        return Enumerable.Empty<FieldInfo>();
    }

    public override bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = ToString();
        return true;
    }

    public override DumpObjectValue<T> ReadAs<T>()
    {
        return DumpObjectValue<T>.CreateFromFailedDumpObject(ToString());
    }

    public override DumpObjectValue<string> AsString(int maxLength)
    {
        return DumpObjectValue<string>.CreateFromFailedDumpObject(ToString());
    }

    public override IEnumerable<DumpObject> EnumerateArrayItems()
    {
        return Enumerable.Empty<DumpObject>();
    }
}