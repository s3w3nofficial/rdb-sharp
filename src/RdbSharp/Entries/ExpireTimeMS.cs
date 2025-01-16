namespace RdbSharp.Entries;

public class ExpireTimeMs : IEntry
{
    public EntryType Type => EntryType.EXPIRETIME_MS;

    public long Miliseconds { get; init; }

    public ExpireTimeMs(long miliseconds)
    {
        Miliseconds = miliseconds;
    }
}