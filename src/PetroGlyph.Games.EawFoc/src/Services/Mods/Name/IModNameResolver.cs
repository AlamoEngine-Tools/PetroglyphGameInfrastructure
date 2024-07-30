using System.Globalization;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Service to resolve a mods's name
/// </summary>
public interface IModNameResolver
{
    /// <summary>
    /// Resolves a culture invariant name of the game.
    /// </summary>
    /// <param name="modReference">The game which name shall get resolved.</param>
    /// <param name="culture">The culture context.</param>
    /// <returns>The resolved name. May return <see langword="null"/> if no name for the <paramref name="culture"/> could be found.</returns>
    /// <exception cref="ModException">The mod's name could not be resolved.</exception>
    string ResolveName(IModReference modReference, CultureInfo culture);
}