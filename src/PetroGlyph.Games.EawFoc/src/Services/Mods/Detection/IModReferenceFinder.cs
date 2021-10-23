using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

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