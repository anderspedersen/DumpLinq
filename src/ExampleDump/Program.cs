// Create Foo objects for Example Dump

using ExampleDump;

Foo[] foos =
[
    new Foo([], new DateTime(2020, 1, 1), "Empty list"),
    new Foo(["A", "B", "C"], new DateTime(2022, 1, 1), "No Bar"),
    new Foo(["A", "B", "C", "Bar"], new DateTime(2024, 1, 1), "With Bar"),

];

Console.ReadLine(); // Now you can create dump

GC.KeepAlive(foos); // Don't eliminate

