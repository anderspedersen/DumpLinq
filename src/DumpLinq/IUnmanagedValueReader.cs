namespace DumpLinq;

internal interface IUnmanagedValueReader
{
    T Read<T>() where T : unmanaged;
}