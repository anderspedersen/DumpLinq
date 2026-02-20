<Query Kind="Expression">
  <NuGetReference>DumpLinq.LINQPad</NuGetReference>
  <Namespace>DumpLinq</Namespace>
  <Namespace>DumpLinq.LINQPad</Namespace>
</Query>

var dump = Dump.Open(@"d:\ExampleDump.dmp");

var instances = dump.EnumerateHeapObjects("*.Foo")
    .Where(foo => foo
        .GetField("_strings")
        .GetField("_items")
        .EnumerateArrayItems()
        .Any(item => item.AsString() == "Bar")).Select(x => x.ToDump()).Dump();

