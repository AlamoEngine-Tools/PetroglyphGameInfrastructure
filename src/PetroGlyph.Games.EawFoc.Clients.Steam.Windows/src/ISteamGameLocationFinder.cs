namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ISteamGameFinder
{
    SteamAppManifest? FindGame(uint gameId);
}