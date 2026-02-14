namespace ExampleDump;

class Foo
{
    List<string> _strings;
    DateTime _time;
    string _title;

    public Foo(List<string> strings, DateTime time, string title)
    {
        _strings = strings;
        _time = time;
        _title = title;
    }
}