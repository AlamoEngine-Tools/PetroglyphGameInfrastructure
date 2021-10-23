namespace PetroGlyph.Games.EawFoc.Clients;

public class GameStartException : ClientException
{
    public IPlayableObject Instance { get; }

    public GameStartException(IPlayableObject instance)
    {
        Instance = instance;
    }
}