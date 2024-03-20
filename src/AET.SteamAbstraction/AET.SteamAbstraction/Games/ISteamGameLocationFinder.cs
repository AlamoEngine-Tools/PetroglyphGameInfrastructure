using System;

namespace AET.SteamAbstraction.Games;

internal interface ISteamGameFinder : IDisposable
{
    SteamAppManifest? FindGame(uint gameId);
}