using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

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
    /// <returns>The location information of the <paramref name="mod"/>.</returns>
    /// <exception cref="NotSupportedException">If the <paramref name="mod"/> is a virtual mod.</exception>
    /// <exception cref="PetroglyphException">If some constraints of the given params do not hold.</exception>
    /// <exception cref="SteamException">If something Steam-related fails.</exception>
    IDirectoryInfo ResolveLocation(IModReference mod, IGame game);
}