namespace RdbSharp.Entries;

/// <summary>
/// Select db entry type
/// </summary>
public class SelectDb : IEntry
{
    public EntryType Type => EntryType.SELECT_DB;

    public long DbIndex { get; init; }

    public SelectDb(long dbIndex)
    {
        DbIndex = dbIndex;
    }
}