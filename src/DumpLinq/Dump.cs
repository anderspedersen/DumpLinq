using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DumpLinq.DumpObjects;
using Microsoft.Diagnostics.Runtime;

namespace DumpLinq;

public sealed class Dump : IDisposable
{
    private readonly DataTarget _dataTarget;
    private readonly ClrInfo _runtimeInfo;
    private readonly ClrRuntime _runtime;
    
    private readonly Dictionary<string, Func<IUnmanagedValueReader, string?>> _valueRenders = new Dictionary<string, Func<IUnmanagedValueReader, string?>>();
    internal DumpObjectFactory Factory { get; }

    private Dump(DataTarget dataTarget, ClrInfo runtimeInfo, ClrRuntime runtime)
    {
        _dataTarget = dataTarget;
        _runtimeInfo = runtimeInfo;
        _runtime = runtime;

        Factory = new DumpObjectFactory(this);
    }

    /// <summary>
    /// Loads a dump file.
    /// </summary>
    /// <param name="dumpPath">The path to the dump file.</param>
    /// <returns>A <see cref="Dump"/> for the given dump file.</returns>
    public static Dump Open(string dumpPath)
    {
        var dataTarget = DataTarget.LoadDump(dumpPath);
        var runtimeInfo = dataTarget.ClrVersions[0];
        var runtime = runtimeInfo.CreateRuntime();

        return new Dump(dataTarget, runtimeInfo, runtime)
            .RegisterFormatter<Guid>()
            .RegisterFormatter<DateTime>();
    }
    
    /// <summary>
    /// Registers a formatter for type <typeparamref name="T"/>. Will use to read and print any object in dump with matching name.
    /// </summary>
    /// <typeparam name="T">The type to register formatter for.</typeparam>
    public Dump RegisterFormatter<T>() where T : unmanaged
    {
        var type = typeof(T);
        
        _valueRenders[type.FullName!] = CreateReaderFunc<T>();
        
        return this;
    }
    
    /// <summary>
    /// Enumerates all heap objects and returns a <see cref="DumpObject"/> for all that matches pattern.
    /// </summary>
    /// <param name="classNamePattern">Class name pattern. Supports wildcards (* and ?).</param>
    /// <returns>An enumerator for all matching heap objects.</returns>
    public IEnumerable<DumpObject> EnumerateHeapObjects(string classNamePattern)
    {
        string regexPattern = "^" +
                              Regex.Escape(classNamePattern)
                                  .Replace(@"\*", ".*")
                                  .Replace(@"\?", ".") +
                              "$";
        
        var regex = new Regex(regexPattern);
        
        return EnumerateHeapObjects(regex);
    }
    
    /// <summary>
    /// Enumerates all heap objects and returns a <see cref="DumpObject"/> for all that matches regular expression.
    /// </summary>
    /// <param name="classNamePattern">Regular expression to match class names against.</param>
    /// <returns>An enumerator for all matching heap objects.</returns>
    public IEnumerable<DumpObject> EnumerateHeapObjects(Regex classNamePattern)
    {
        Dictionary<ulong, bool> matchCache = new Dictionary<ulong, bool>();
        
        foreach (ClrObject obj in _runtime.Heap.EnumerateObjects())
        {
            // If heap corruption, continue past this object.
            if (!obj.IsValid)
                continue;

            if (!matchCache.TryGetValue(obj.Type.MethodTable, out var isMatch))
            {
                isMatch = classNamePattern.IsMatch(obj.Type.Name);
                matchCache[obj.Type.MethodTable] = isMatch;
            }
            
            if (!isMatch)
                continue;

            yield return Factory.CreateDumpObject(obj);
        }
    }

    private Func<IUnmanagedValueReader, string?> CreateReaderFunc<T>() where T : unmanaged
    {
        return r => r.Read<T>().ToString();
    }

    internal bool TryGetValueRender(string typeName, [MaybeNullWhen(false)]out Func<IUnmanagedValueReader, string?> renderer)
    {
        return _valueRenders.TryGetValue(typeName, out renderer);
    }

    public void Dispose()
    {
        _runtime.Dispose();
        _dataTarget.Dispose();
    }
}