using DumpLinq;

using var dump = Dump.Open(@"d:\ExampleDump.dmp");
var instances = dump.EnumerateHeapObjects("*.Foo")
    .Where(foo => foo
        .GetField("_strings")
        .GetField("_items")
        .EnumerateArrayItems()
        .Any(item => item.AsString(4096) == "Bar"));

foreach (var instance in instances)
{
    Console.WriteLine(instance.ToString());
}