namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

internal interface IArgumentCommandLineBuilder
{
    string BuildCommandLine(IArgumentCollection arguments);
}