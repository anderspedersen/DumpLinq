using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DumpLinq.DumpObjects;

internal class NullDumpObject : DumpObject
{
    private readonly string _name;
    private readonly DumpObject _parent;

    public NullDumpObject(string name, DumpObject parent)
    {
        _name = name;
        _parent = parent;
    }

    public override DumpObject GetField(string name)
    {
        return new NullDumpObject(_name + "." + name, _parent);
    }
    
    protected internal override void BuildString(StringBuilder sb, int depth, int arrayItems, int indention)
    {
        if (indention == 0) // Root object, print all details
        {
            sb.Append("Field was null: ")
                .Append(_name)
                .Append(" in ");
            _parent.BuildString(sb, 0, 0, 0);
        }
        else
        {
            sb.Append("null");
        }
    }

    public override ulong Address => 0;
    public override bool TryRenderValue([NotNullWhen(true)] out string? value)
    {
        value = "null";
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