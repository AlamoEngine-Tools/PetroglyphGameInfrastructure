using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamGameLocationFinder
    {
        IDirectoryInfo? FindGame(IDirectoryInfo steamInstallationDirectory, uint gameId);
    }
}