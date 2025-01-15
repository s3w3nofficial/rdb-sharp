namespace RdbSharp.Entries;

public class EOF : IEntry
{
    public EntryType Type => EntryType.EOF;
}