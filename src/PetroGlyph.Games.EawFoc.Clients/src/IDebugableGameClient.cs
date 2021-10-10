using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;

namespace PetroGlyph.Games.EawFoc.Clients
{
    public interface IDebugableGameClient : IGameClient
    {
        bool IsDebugAvailable(IPlayableObject instance);

        IGameProcess Debug(IPlayableObject instance);

        IGameProcess Debug(IPlayableObject instance, IGameArgumentCollection arguments, bool fallbackToPlay);
    }
}