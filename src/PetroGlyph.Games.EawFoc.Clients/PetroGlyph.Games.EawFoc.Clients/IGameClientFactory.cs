using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients
{
    public interface IGameClientFactory
    {
        IGameClient CreateClient(GamePlatform gamePlatform);
    }
}