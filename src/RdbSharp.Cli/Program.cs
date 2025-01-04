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
}