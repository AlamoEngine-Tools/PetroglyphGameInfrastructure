using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to search for <see cref="IModReference"/>s.
/// </summary>
public interface IModFinder
{
    /// <summary>
    /// Searches for <see cref="IModReference"/> for a given game.
    /// </summary>
    /// <param name="game">The game to search mods for.</param>
    /// <returns>A set of detected mod references.</returns>
    ISet<DetectedModReference> FindMods(IGame game);
}