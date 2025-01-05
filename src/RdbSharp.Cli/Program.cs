using RdbSharp;

if (args.Length < 2)
{
    Console.WriteLine("Usage: RdbSharp <path-to-rdb> <format>");
    return;
}

var rdbPath = args[0];

var parser = new RdbSharpParser(rdbPath);

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
        case EntryType.KV:
        {
            break;
        }
    }
}