using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <summary>
/// Common helpers for Steam-based Games
/// </summary>
internal interface ISteamGameHelpers
{
    /// <summary>
    /// Gets the game's workshop directory.
    /// </summary>
    /// <param name="game">The target game</param>
    /// <exception cref="GameException">If the game is not a Steam game</exception>
    /// <exception cref="GameException">If it was impossible to compute the workshop location.</exception>
    IDirectoryInfo GetWorkshopsLocation(IGame game);

    /// <summary>
    /// Tries to get the game's workshop location.
    /// </summary>
    /// <param name="game">The target game.</param>
    /// <param name="workshopsLocation">The found location; <see langword="null"/> of no workshop location could be computed.</param>
    /// <returns><see langword="true"/>if the workshop location was found; <see langword="false"/> otherwise.</returns>
    bool TryGetWorkshopsLocation(IGame game, out IDirectoryInfo? workshopsLocation);

    /// <summary>
    /// Tries to convert a string to a <see cref="ulong"/> value which acts as a SteamWorkshop ID. 
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="steamId">The resulting id.</param>
    /// <returns><see langword="true"/>if the <paramref name="input"/> could be converted; <see langword="false"/> otherwise.</returns>
    bool ToSteamWorkshopsId(string input, out ulong steamId);
}