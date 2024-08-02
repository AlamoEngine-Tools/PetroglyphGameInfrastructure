using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service to search for a physical location of a given <see cref="IModReference"/>.
/// </summary>
public interface IModReferenceLocationResolver
{
    /// <summary>
    /// Searches the location of the <paramref name="mod"/>. Virtual mods are not supported.
    /// </summary>
    /// <param name="mod">The <see cref="IModReference"/> to lookup.</param>
    /// <param name="game">The reference <see cref="IGame"/> of the <paramref name="mod"/>.</param>
    /// <param name="checkModGameType">When set to <see langword="true"/> a Steam workshop mod will be checked if ít is associated to <paramref name="game"/>.</param>
    /// <returns>The location information of the <paramref name="mod"/>.</returns>
    /// <exception cref="NotSupportedException">If the <paramref name="mod"/> is a virtual mod.</exception>
    /// <exception cref="ModNotFoundException"><paramref name="mod"/> could not be found.</exception>
    /// <exception cref="ModException"><paramref name="mod"/> has an invalid ID or Steam Workshop game type validation failed.</exception>
    IDirectoryInfo ResolveLocation(IModReference mod, IGame game, bool checkModGameType);
}