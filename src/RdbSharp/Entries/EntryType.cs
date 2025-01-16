namespace RdbSharp.Entries;

/// <summary>
/// entry type
/// </summary>
public enum EntryType
{
    EOF,
    SELECT_DB,
    KV,
    RESIZE_DB,
    AUX,
    EXPIRETIME,
    EXPIRETIME_MS
}