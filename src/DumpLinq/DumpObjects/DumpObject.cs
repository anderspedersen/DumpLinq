using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DumpLinq.DumpObjects;

/// <summary>
/// Represents an object on the heap.
/// </summary>
public abstract class DumpObject
{   
    /// <summary>
    /// Gets a <see cref="DumpObject"/> representing the value of the field.
    /// If the field does not exist, a failed <see cref="DumpObject"/> is returned.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>A <see cref="DumpObject"/> representing the value of the field</returns>
    public abstract DumpObject GetField(string name);

    protected internal abstract void BuildString(StringBuilder sb, int depth, int arrayItems, int indention);
    
    /// <summary>
    /// Gets the memory address of the object represented by this <see cref="DumpObject"/>.
    /// Returns 0 if this <see cref="DumpObject"/> represents a null reference, and ulong.MaxValue if it is a failed <see cref="DumpObject"/>.
    /// </summary>
    /// <returns>The memory address of the object</returns>
    public abstract ulong Address { get; }

    /// <summary>
    /// Tries to render the value of this <see cref="DumpObject"/> as a string.
    /// Will succeed if the <see cref="DumpObject"/> represents a string, a primitive type or a struct where
    /// a formatter has been registered using <see cref="Dump.RegisterFormatter"/>.
    /// </summary>
    /// <param name="value">The value rendered as a string, or null if value couldn't be rendered.</param>
    /// <returns>True if value could be rendered; otherwise, false</returns>
    public abstract bool TryRenderValue([NotNullWhen(true)] out string? value);
    
    /// <summary>
    /// Tests if this <see cref="DumpObject"/> represents an array.
    /// </summary>
    /// <returns>True <see cref="DumpObject"/> represents an array; otherwise, false</returns>
    public abstract bool IsArray();
    
    /// <summary>
    /// Enumerates all fields in object represented by this <see cref="DumpObject"/>.
    /// </summary>
    /// <returns>True <see cref="DumpObject"/> represents an array; otherwise, false</returns>
    public abstract IEnumerable<FieldInfo> GetFields();
    
    /// <summary>
    /// Tries to get the error message of this <see cref="DumpObject"/>.
    /// Will only succeed if this is a failed <see cref="DumpObject"/>.
    /// </summary>
    /// <param name="error">The error messages of this <see cref="DumpObject"/>, or null if this is not a failed <see cref="DumpObject"/>.</param>
    /// <returns>True if this is a failed <see cref="DumpObject"/>; otherwise, false</returns>
    public abstract bool TryGetError([NotNullWhen(true)] out string? error);
    
    /// <summary>
    /// Reads the value represented by this <see cref="DumpObject"/> as type <typeparamref name="T"/>.
    /// <typeparam name="T">An unmanaged value type whose size matches the underlying value.</typeparam>
    /// </summary>
    /// A <see cref="DumpObjectValue{T}"/> containing the value if the read succeeds;
    /// otherwise a failed result.
    /// <remarks>
    /// This method can only be used when the DumpObject represents a value type.
    /// The target type <typeparamref name="T"/> must be unmanaged and have the same size
    /// as the underlying value.
    /// </remarks>
    public abstract DumpObjectValue<T> ReadAs<T>() where T : unmanaged;

    /// <summary>
    /// Reads the value represented by this <see cref="DumpObject"/> as a string.
    /// </summary>
    /// <param name="maxLength">The maximum number of characters to read from the string.  
    /// If the string is longer, the result will be truncated to this length.  </param>
    /// <returns>
    /// A <see cref="DumpObjectValue{string}"/> containing the string value if successful;
    /// otherwise a failed result.
    /// </returns>
    /// <remarks>
    /// This method can only be used when the DumpObject represents a string or an enum.
    /// For enums, the returned string is the enum's name.  
    /// For all other types, the result will be failed.
    /// </remarks>
    public abstract DumpObjectValue<string> AsString(int maxLength = 4096);
    
    /// <summary>
    /// Enumerates all elements of this <see cref="DumpObject"/> if it represents an array.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{DumpObject}"/> containing each item in the array.
    /// If this <see cref="DumpObject"/> does not represent an array, returns an empty sequence.
    /// </returns>
    public abstract IEnumerable<DumpObject> EnumerateArrayItems();

    /// <summary>
    /// Returns a tree-like string representation of the <see cref="DumpObject"/> and its fields.
    /// </summary>
    /// <param name="depth">
    /// The maximum depth of nested objects to include in the output.  
    /// Defaults to <c>3</c>. Objects deeper than this level will not be expanded.
    /// </param>
    /// <param name="arrayItems">
    /// The maximum number of elements to include when representing arrays.  
    /// Defaults to <c>5</c>. Arrays longer than this limit will be truncated.
    /// </param>
    /// <returns>
    /// A string showing the object and its fields in a tree structure, 
    /// expanding nested objects up to the specified depth and truncating arrays at the specified limit.
    /// </returns>
    public string ToString(int depth = 3, int arrayItems = 5)
    {
        var sb = new StringBuilder();
        BuildString(sb, depth, arrayItems, indention: 0);
        return sb.ToString();
    }
}