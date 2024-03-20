using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to search for <see cref="IModReference"/>s.
/// </summary>
public interface IModReferenceFinder
{
    /// <summary>
    /// Searches for <see cref="IModReference"/> for a given game
    /// </summary>
    /// <param name="game">The target game.</param>
    /// <returns>A set of found <see cref="IModReference"/>s.</returns>
    ISet<IModReference> FindMods(IGame game);
}