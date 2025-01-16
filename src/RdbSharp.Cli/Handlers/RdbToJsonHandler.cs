using System.Text.Json;
using RdbSharp.Entries;
using KeyValuePair = RdbSharp.Entries.KeyValuePair;

namespace RdbSharp.Cli.Handlers;

public class RdbToJsonHandler : IHandler 
{
    public void Handle(RdbSharpParser parser)
    {
        var entires = new Dictionary<string, object>();
        
        IEntry? entry;
        while ((entry = parser.NextEntry()) != null)
        {
            switch (entry.Type)
            {
                case EntryType.EOF:
                case EntryType.SELECT_DB:
                case EntryType.RESIZE_DB:
                case EntryType.AUX:
                    break;
                case EntryType.KV:
                {
                    var kv = (KeyValuePair)entry;

                    switch (kv.RdbType)
                    {
                        case RdbType.STRING:
                        {
                            var value = (string) kv.Value;
                            entires.Add(kv.Key, value);
                            break;
                        }
                        case RdbType.LIST:
                        {
                            var items = (List<string>)kv.Value;
                            entires.Add(kv.Key, items);
                            break;
                        }
                        case RdbType.SET:
                        {
                            var items = (List<string>)kv.Value;
                            entires.Add(kv.Key, items);
                            break;
                        }
                        case RdbType.LIST_QUICKLIST_2:
                        {
                            var items = (List<string>)kv.Value;
                            entires.Add(kv.Key, items);
                            break;
                        }
                        case RdbType.SET_LISTPACK:
                        {
                            var items = (List<string>)kv.Value;
                            entires.Add(kv.Key, items);
                            break;
                        }
                        default:
                            break;
                    }

                    break;
                }
            }
        }

        Console.WriteLine(JsonSerializer.Serialize(entires, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}