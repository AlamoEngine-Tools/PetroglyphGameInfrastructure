using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <summary>
/// Provides common helpers for Steam-based Games
/// </summary>
public interface ISteamGameHelpers
{
    /// <summary>
    /// Gets the game's Steam Workshop directory.
    /// </summary>
    /// <remarks>The method does not check whether the Steam Workshop directory actually exists.</remarks>
    /// <param name="game">The game to get the Steam Workshop directory for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException"><paramref name="game"/> is not a Steam game.</exception>
    /// <exception cref="GameException">It is not impossible to get the Steam Workshop path for <paramref name="game"/>.</exception>
    IDirectoryInfo GetWorkshopsLocation(IGame game);

    /// <summary>
    /// Tries to get the game's workshop location.
    /// </summary>
    /// <remarks>The method does not check whether the Steam Workshop directory actually exists.</remarks>
    /// <param name="game">The target game.</param>
    /// <param name="workshopsLocation">The found location; <see langword="null"/> of no workshop location could be computed.</param>
    /// <returns><see langword="true"/>if the Steam Workshop path could be determined; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    bool TryGetWorkshopsLocation(IGame game, [NotNullWhen(true)] out IDirectoryInfo? workshopsLocation);

    /// <summary>
    /// Tries to convert a string to a <see cref="ulong"/> value which represents a Steam Workshop ID. 
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="steamId">The resulting id.</param>
    /// <returns><see langword="true"/>if the <paramref name="input"/> could be converted; otherwise, <see langword="false"/>.</returns>
    bool ToSteamWorkshopsId(string input, out ulong steamId);
}