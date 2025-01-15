namespace RdbSharp.Cli.Handlers;

public interface IHandler
{
    void Handle(RdbSharpParser parser);
}