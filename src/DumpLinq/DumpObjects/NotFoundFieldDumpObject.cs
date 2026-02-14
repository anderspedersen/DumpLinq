using System.Text;

namespace DumpLinq.DumpObjects;

internal class NotFoundFieldDumpObject : DumpObject
{
    private readonly string _name;
    private readonly DumpObject _parent;

    public NotFoundFieldDumpObject(string name, DumpObject parent)
    {
        _name = name;
        _parent = parent;
    }

    public override DumpObject GetField(string name)
    {
        return new NotFoundFieldDumpObject(_name + "." + name, _parent);
    }

    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        sb.Append("Field not found: ")
            .Append(_name)
            .Append(" in ");
        _parent.BuildString(sb, 0, 0, 0);
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