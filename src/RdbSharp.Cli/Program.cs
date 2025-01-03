using RdbSharp;

if (args.Length < 1)
{
    Console.WriteLine("Usage: RdbSharp <path-to-rdb>");
    return;
}

var rdbPath = args[0];

Parser.Parse(rdbPath);
