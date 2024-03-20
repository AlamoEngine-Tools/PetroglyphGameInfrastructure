namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

internal interface IArgumentCommandLineBuilder
{
    string BuildCommandLine(IArgumentCollection arguments);
}