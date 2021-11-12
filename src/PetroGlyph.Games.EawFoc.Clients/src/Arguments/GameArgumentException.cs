namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public class GameArgumentException : PetroglyphException
{
    public GameArgumentException(IGameArgument argument)
    {
    }

    public GameArgumentException(IGameArgument argument, string message) : base(message)
    {
    }
}