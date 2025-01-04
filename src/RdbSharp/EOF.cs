namespace RdbSharp;

public class EOF : IEntry
{
    public EntryType Type => EntryType.EOF;
}