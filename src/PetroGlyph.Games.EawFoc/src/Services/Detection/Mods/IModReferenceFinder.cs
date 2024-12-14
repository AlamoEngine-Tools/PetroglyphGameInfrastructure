using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to search for <see cref="IModReference"/>s.
/// </summary>
public interface IModFinder
{
    /// <summary>
    /// Searches for physically installed mods for the specified game.
    /// </summary>
    /// <param name="game">The game to search mods for.</param>
    /// <returns>A collection of installed mods of <paramref name="game"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException"><paramref name="game"/> does not exist.</exception>
    ICollection<DetectedModReference> FindMods(IGame game);
}