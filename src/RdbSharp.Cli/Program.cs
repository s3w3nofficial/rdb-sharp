using RdbSharp;

if (args.Length < 1)
{
    Console.WriteLine("Usage: SimpleRedisRdbParser <path-to-rdb>");
    return;
}

var rdbPath = args[0];

var parser = new Parser(rdbPath);

parser.Parse();
