namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public interface ISteamGameFinder
{
    SteamAppManifest? FindGame(uint gameId);
}