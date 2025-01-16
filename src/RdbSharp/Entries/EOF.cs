namespace RdbSharp.Entries;

/// <summary>
/// EOF entry type
/// </summary>
public class EOF : IEntry
{
    public EntryType Type => EntryType.EOF;
}