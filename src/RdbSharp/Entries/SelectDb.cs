namespace RdbSharp.Entries;

public class SelectDb : IEntry
{
    public EntryType Type => EntryType.SELECT_DB;

    public long DbIndex { get; init; }

    public SelectDb(long dbIndex)
    {
        DbIndex = dbIndex;
    }
}