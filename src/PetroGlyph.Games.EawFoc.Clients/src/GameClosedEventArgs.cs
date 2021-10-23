namespace PetroGlyph.Games.EawFoc.Clients;

public class GameClosedEventArgs
{
    public IPlayableObject PlayedInstance { get; }

    public GameClosedEventArgs(IPlayableObject playedInstance)
    {
        PlayedInstance = playedInstance;
    }
}