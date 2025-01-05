namespace RdbSharp;

public class ResizeDb : IEntry
{
    public EntryType Type => EntryType.RESIZE_DB;

    public long DbIndex { get; init; }

    public long ExpireSize { get; init; }

    public ResizeDb(long dbIndex, long expireSize)
    {
        DbIndex = dbIndex;
        ExpireSize = expireSize;
    }
}