using System.Dynamic;
using DumpLinq.DumpObjects;

namespace DumpLinq.LINQPad;

/// <summary>
/// Extension methods for <see cref="DumpObject"/> to use with tools such as LINQPad.
/// </summary>
public static class DumpObjectExtensions
{
    /// <summary>
    /// Returns a tree representation of this <see cref="DumpObject"/>suitable for visualization in tools such as LINQPad.
    /// </summary>
    /// <param name="dumpObject">The object to visualize</param>
    /// <param name="depth">The maximum number of levels to expand in the tree. Nested objects beyond this depth may be expanded lazily.</param>
    /// <returns>An object representing the tree structure of <paramref name="dumpObject"/>, which can be inspected or displayed in visualization tools.</returns>
    public static object ToDump(this DumpObject dumpObject, int depth = 3)
    {
        return ToDumpInternal(dumpObject, depth);
    }

    private static object ToDumpInternal(DumpObject dumpObject, int depth)
    {
        if (dumpObject.TryRenderValue(out var value))
        {
            return value;
        }

        if (dumpObject.TryGetError(out var error))
        {
            return error;
        }
        
        return depth <= 0 
            ? new Lazy<object>(() => ToExpando(dumpObject, depth))
            : ToExpando(dumpObject, depth);
    }

    private static object ToExpando(DumpObject dumpObject, int depth)
    {
        IDictionary<string, object?> expando = new ExpandoObject();
        foreach (var field in dumpObject.GetFields())
        {
            expando[field.Name] = ToDumpInternal(dumpObject.GetField(field.Name), depth - 1);
        }

        if (dumpObject.IsArray())
        {
            expando["Items"] = dumpObject.EnumerateArrayItems().Select(x => ToDumpInternal(x, depth - 1));
        }
        
        return expando;
    }
}