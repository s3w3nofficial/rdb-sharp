using RdbSharp;
using RdbSharp.Cli.Handlers;

if (args.Length < 2)
{
    Console.WriteLine("Usage: RdbSharp.Cli <path-to-rdb> <format>");
    return;
}

var rdbPath = args[0];
var format = args[1];

var parser = new RdbSharpParser(rdbPath);

switch (format.ToLowerInvariant())
{
    case "print":
    {
        var handler = new RdbToPrintHandler();
        handler.Handle(parser);
        break;
    }
    case "json":
    {
        var handler = new RdbToJsonHandler();
        handler.Handle(parser);
        break;
    }
    case "resp":
    {
        var handler = new RdbToRespHandler();
        handler.Handle(parser);
        break;
    }
    default:
        break;
}