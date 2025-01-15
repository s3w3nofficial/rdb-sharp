namespace RdbSharp.Entries;

public class KeyValuePair : IEntry
{
    public EntryType Type => EntryType.KV;
    
    public  RdbType RdbType { get; set; }

    public string Key { get; init; }
    
    public object Value { get; init; }

    public KeyValuePair(string key, object value, RdbType rdbType)
    {
        Key = key;
        Value = value;
        RdbType = rdbType;
    }
}