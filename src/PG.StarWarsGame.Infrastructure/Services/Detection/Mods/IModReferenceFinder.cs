using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.Modinfo.Spec;
using AET.Modinfo.Utilities;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to search for <see cref="IModReference"/>s.
/// </summary>
public interface IModFinder
{
    /// <summary>
    /// Searches for physically installed mods compatible for the specified game.
    /// </summary>
    /// <param name="game">The game to search mods for.</param>
    /// <returns>A collection of installed mods compatible for <paramref name="game"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException"><paramref name="game"/> does not exist.</exception>
    IEnumerable<DetectedModReference> FindMods(IGame game);

    /// <summary>
    /// Searches for physically installed mods compatible for the specified game at the specified directory.
    /// </summary>
    /// <remarks>
    /// <paramref name="directory"/> is treated as the root directory of a mod.
    /// The returned collection contains at least one mod reference,
    /// if <paramref name="directory"/> exists and the containing mod is compatible to <paramref name="game"/>.
    /// The returned collection may contain multiple mod references due to the layout of existing modinfo files.
    /// </remarks>
    /// <param name="game">The game to search mods for.</param>
    /// <param name="directory">The directory to search mods for.</param>
    /// <returns>A collection of installed mods of <paramref name="game"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="directory"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException"><paramref name="game"/> does not exist.</exception>
    IEnumerable<DetectedModReference> FindMods(IGame game, IDirectoryInfo directory);
}