namespace AET.SteamAbstraction.Games;

internal interface ISteamGameFinder
{
    SteamAppManifest? FindGame(uint gameId);
}