namespace DumpLinq.DumpObjects;

/// <summary>
/// Information about a field of a <see cref="DumpObject"/>
/// </summary>
public struct FieldInfo
{
    /// <summary>
    /// Name of the field.
    /// </summary>
    public string Name { get; }

    internal FieldInfo(string name)
    {
        Name = name;
    }
}