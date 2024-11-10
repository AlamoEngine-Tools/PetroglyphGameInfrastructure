using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <summary>
/// Provides common helpers for Steam-based Games
/// </summary>
public interface ISteamGameHelpers
{
    /// <summary>
    /// Gets the game's workshop directory.
    /// </summary>
    /// <param name="game">The target game</param>
    /// <exception cref="ArgumentException"><paramref name="game"/> is not a Steam game.</exception>
    /// <exception cref="GameException">It was impossible to compute the workshop location for <paramref name="game"/>.</exception>
    IDirectoryInfo GetWorkshopsLocation(IGame game);

    /// <summary>
    /// Tries to get the game's workshop location.
    /// </summary>
    /// <param name="game">The target game.</param>
    /// <param name="workshopsLocation">The found location; <see langword="null"/> of no workshop location could be computed.</param>
    /// <returns><see langword="true"/>if the workshop location was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetWorkshopsLocation(IGame game, out IDirectoryInfo? workshopsLocation);

    /// <summary>
    /// Tries to convert a string to a <see cref="ulong"/> value which represents a Steam Workshop ID. 
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="steamId">The resulting id.</param>
    /// <returns><see langword="true"/>if the <paramref name="input"/> could be converted; otherwise, <see langword="false"/>.</returns>
    bool ToSteamWorkshopsId(string input, out ulong steamId);
}