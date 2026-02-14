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