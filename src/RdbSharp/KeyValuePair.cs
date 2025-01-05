namespace RdbSharp;

public class KeyValuePair : IEntry
{
    public EntryType Type => EntryType.KV;
    
    public ValueType ValueType { get; set; }

    public string Key { get; init; }
    
    public object Value { get; init; }

    public KeyValuePair(string key, object value, ValueType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }
}