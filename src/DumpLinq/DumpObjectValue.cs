using System.Runtime.CompilerServices;

namespace DumpLinq;

public readonly struct DumpObjectValue<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private DumpObjectValue(T? value, string? error)
    {
        _value = value;
        _error = error;
    }
    
    /// <summary>
    /// Indicates whether the value could not be read and an error occurred.
    /// </summary>
    public bool IsError => _error is not null;
    
    /// <summary>
    /// The error message if reading the value failed; otherwise, <c>null</c>.
    /// </summary>
    public string? Error => _error;
    
    /// <summary>
    /// The successfully read value of type <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="IsError"/> is <c>true</c>. Use <see cref="Error"/> to see the failure reason.
    /// </exception>
    public T Value  => _error is null ? _value! : throw new InvalidOperationException(_error);
    
    /// <summary>
    /// Implicitly converts a <see cref="DumpObjectValue{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The <see cref="DumpObjectValue{T}"/> instance.</param>
    /// <returns>The underlying value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="DumpObjectValue{T}"/> represents an error.
    /// </exception>
    public static implicit operator T(DumpObjectValue<T> value) => value.Value;
    
    internal static DumpObjectValue<T> CreateFailedUnsupportedTypeForValueType(string typeName)
    {
        return new DumpObjectValue<T>(default(T), $"ReadAs<T>() can only be used on value types. The current DumpObject represents {typeName}.");
    }
    
    internal static DumpObjectValue<T> CreateFailedUnsupportedTypeForString(string typeName)
    {
        return new DumpObjectValue<T>(default(T), $"AsString() can only be used on string or enum types. The current DumpObject represents {typeName}.");
    }

    internal static DumpObjectValue<T> Create(T value)
    {
        return new DumpObjectValue<T>(value, null);
    }

    internal static DumpObjectValue<T> CreateFailedSizesNotMatching(string typeName, int size)
    {

        return new DumpObjectValue<T>(default(T),
            $"ReadAs<{typeof(T).Name}>() failed due to size mismatch. Target type size: {Unsafe.SizeOf<T>()} bytes. Underlying value type {typeName} size: {size} bytes.");
    }

    internal static DumpObjectValue<T> CreateFromFailedDumpObject(string error)
    {
        return new DumpObjectValue<T>(default(T), $"Cannot read from failed DumpObject. Error: {error}");
    }

    internal static DumpObjectValue<T> CreateFailedToFindField0(string typeName)
    {
        return new DumpObjectValue<T>(default(T), $"Failed to find field at offset 0 for value type {typeName}");
    }
}