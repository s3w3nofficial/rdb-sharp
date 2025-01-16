using RdbSharp.Entries;
using KeyValuePair = RdbSharp.Entries.KeyValuePair;

namespace RdbSharp.Cli.Handlers;

public class RdbToPrintHandler : IHandler
{
    public void Handle(RdbSharpParser parser)
    {
        Console.WriteLine($"RDB Version: {parser.Version}");
        
        IEntry? entry;
        while ((entry = parser.NextEntry()) != null)
        {
            switch (entry.Type)
            {
                case EntryType.EOF:
                {
                    Console.WriteLine("Found EOF opcode. Stopping parse.");
                    break;
                }
                case EntryType.SELECT_DB:
                {
                    Console.WriteLine($"Selecting DB {((SelectDb)entry).DbIndex}.");
                    break;
                }
                case EntryType.RESIZE_DB:
                {
                    Console.WriteLine("Found RESIZE DB opcode.");
                    break;
                }
                case EntryType.AUX:
                {
                    var aux = (Aux)entry;
                    Console.WriteLine(aux.ToString());
                    break;
                }
                case EntryType.EXPIRETIME:
                {
                    var expire = (ExpireTime)entry;
                    Console.WriteLine("Found EXPIRETIME opcode.");
                    break;
                }
                case EntryType.EXPIRETIME_MS:
                {
                    var expire = (ExpireTimeMs)entry;
                    Console.WriteLine($"Found EXPIRETIME MS opcode with: {expire.Miliseconds}.");
                    break;
                }
                case EntryType.KV:
                {
                    var kv = (KeyValuePair)entry;

                    Console.WriteLine(kv.RdbType);

                    switch (kv.RdbType)
                    {
                        case RdbType.STRING:
                        {
                            var value = (string) kv.Value;
                            Console.WriteLine($"  Value (String): {value}");
                            break;
                        }
                        case RdbType.LIST:
                        {
                            var items = (List<string>)kv.Value;
                            Console.WriteLine($"  List length: {items.Count}");

                            foreach (var (i, item) in items.Index())
                            {
                                Console.WriteLine($"    List item {i}: {item}");
                            }
                            break;
                        }
                        case RdbType.SET:
                        {
                            var items = (List<string>)kv.Value;
                            Console.WriteLine($"  Set length: {items.Count}");
                            
                            foreach (var (i, item) in items.Index())
                            {
                                Console.WriteLine($"    List item {i}: {item}");
                                Console.WriteLine($"    Set item {i}: {item}");
                            }
                            
                            break;
                        }
                        case RdbType.LIST_QUICKLIST_2:
                        {
                            var items = (List<string>)kv.Value;
                            Console.WriteLine($"  List length: {items.Count}");

                            foreach (var (i, item) in items.Index())
                            {
                                Console.WriteLine($"    List item {i}: {item}");
                            }
                            break;
                        }
                        case RdbType.SET_LISTPACK:
                        {
                            var items = (HashSet<string>)kv.Value;
                            Console.WriteLine($"  Set length: {items.Count}");

                            foreach (var (i, item) in items.Index())
                            {
                                Console.WriteLine($"    Set item {i}: {item}");
                            }

                            break;
                        }
                        default:
                            break;
                    }

                    break;
                }
            }
        }
    }
}