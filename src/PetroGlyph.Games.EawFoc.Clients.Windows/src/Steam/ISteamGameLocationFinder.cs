using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public interface ISteamGameFinder
{
    SteamAppManifest? FindGame(IDirectoryInfo steamInstallationDirectory, uint gameId);
}