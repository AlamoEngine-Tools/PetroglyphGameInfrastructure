using System.Globalization;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Name;

/// <summary>
/// Service to resolve a mods's name
/// </summary>
public interface IModNameResolver
{
    /// <summary>
    /// Resolves a culture invariant name of the game.
    /// </summary>
    /// <param name="modReference">The game which name shall get resolved.</param>
    /// <returns>The resolved name.</returns>
    /// <exception cref="PetroglyphException">when no valid name could be resolved.</exception>
    string ResolveName(IModReference modReference);

    /// <summary>
    /// Resolves a culture invariant name of the game.
    /// </summary>
    /// <param name="modReference">The game which name shall get resolved.</param>
    /// <param name="culture">The culture context.</param>
    /// <returns>The resolved name. May return <see langword="null"/> if no name name for the <paramref name="culture"/> could be found.</returns>
    string? ResolveName(IModReference modReference, CultureInfo culture);
}