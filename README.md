# DumpLinq
[![NuGet](https://img.shields.io/nuget/v/DumpLinq)](https://www.nuget.org/packages/DumpLinq/)

DumpLinq is a library for querying .NET memory dumps using familiar LINQ syntax. It is built on top of ClrMD and provides higher-level abstractions that simplify navigating the dump.

## Example
Let's say you have the following class in the process you dumped:

```
class Foo
{
    private List<string> _strings;
    private DateTime _time;
    private string _title;
}
```

If you wanted to find and print all instances of Foo in the dump, where `_strings` contains at least one string that is equal to `Bar`, then you could do that with the following code:

```
using var dump = Dump.Open(@"d:\ExampleDump.dmp");
var instances = dump.EnumerateHeapObjects("*.Foo")
        .Where(foo => foo
            .GetField("_strings")
            .GetField("_items")
            .EnumerateArrayItems()
            .Any(item => item.AsString() == "Bar"));

foreach (var instance in instances)
{
    Console.WriteLine(instance.ToString());
}
```

The output is a tree representation of the object graph:
```
ExampleDump.Foo @ 0x000002E5730ABA90
  _strings: System.Collections.Generic.List<System.String> @ 0x000002E5730ABA38
    _items: System.String[] @ 0x000002E5730ABA58
      [0]: "A"
      [1]: "B"
      [2]: "C"
      [3]: "Bar"
    _size: 4
    _version: 1
  _time: 01/01/2024 00.00.00
  _title: "With Bar"
```

## Getting Started

Let's break down the query above to learn how to build your own.

### Enumerating Heap Objects

`IEnumerable<DumpObject> Dump.EnumerateHeapObjects(string pattern)` 

Enumerates all objects that have their own allocation on the managed heap (i.e. reference types and boxed value types, 
but not value types stored inline within other objects).

`EnumerateHeapObjects` expects fully qualified type names (including namespace), but it supports wildcards. If your  
type name is unique, you can use a pattern like `*.Foo`.

`dump.EnumerateHeapObjects("*.Foo")`

This returns a `DumpObject` for every independently allocated object whose full type name matches `*.Foo`.

### Accessing Fields

`DumpObject DumpObject.GetField(string name)` 

Returns a `DumpObject` that represents the object stored in the specified field. 

In DumpLinq, everything on the heap is represented as a `DumpObject`, including 
- Reference types
- Value types
- Boxed value types
- Primitive types
- Arrays
- Enums

For example:

`foo.GetField("_strings")`

Gets the `_strings` field from `Foo`.

`GetField("_items")`

Gets the `_items` backing array from `List<string>`.

### Enumerating Arrays

`IEnumerable<DumpObject> EnumerateArrayItems()` 

Enumerates all elements of a `DumpObject` when it represents an array.

If the object is not an array, it returns an empty sequence.

### Reading Values

`DumpObjectValue<string> DumpObject.AsString()` 

Attempts to read the object as a string
- If the object represents a string the value is returned
- If the object represents an enum, the enum's string representation is returned
- Otherwise, a failed `DumpObjectValue<string>` is returned

To read primitive or unmanaged value types, you can use:

`DumpObjectValue<T> DumpObject.ReadAs<T>()`

`ReadAs<T>` reads the value represented by the `DumpObject` as type `T`. 

`T` doesn't have to match the exact type represented by the `DumpObject`, but the following constraints apply:

- `DumpObject` must be a value type (boxed value types also work)
- `T` must be an unmanaged type (i.e. a primitive type or a struct that doesn't contain any managed references)
- The size of `T` must match the size of the value represented by the `DumpObject`.
- Otherwise, a failed `DumpObjectValue<T>` is returned

To illustrate the use of `ReadAs<T>` we can modify the previous query to return all `Foo` instances where `_strings` 
contains more than 5 elements:

```
using var dump = Dump.Open(@"d:\ExampleDump.dmp");
var instances = dump.EnumerateHeapObjects("*.Foo")
        .Where(foo => foo
            .GetField("_strings")
            .GetField("_size")
            .ReadAs<int>() > 5);
```

### Printing Objects

`string DumpObject.ToString(int depth = 3, int arrayItems = 5)`

Returns a tree-like representation of the object. The `depth` parameter controls the maximum depth of nested objects to 
include, and the `arrayItems` parameter controls the maximum number of elements to include from each array.