namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal interface ISteamGameFinder
{
    SteamAppManifest? FindGame(uint gameId);
}