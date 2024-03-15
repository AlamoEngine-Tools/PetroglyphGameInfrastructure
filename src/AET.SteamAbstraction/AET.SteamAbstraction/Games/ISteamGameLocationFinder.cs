namespace AET.SteamAbstraction;

internal interface ISteamGameFinder
{
    SteamAppManifest? FindGame(uint gameId);
}