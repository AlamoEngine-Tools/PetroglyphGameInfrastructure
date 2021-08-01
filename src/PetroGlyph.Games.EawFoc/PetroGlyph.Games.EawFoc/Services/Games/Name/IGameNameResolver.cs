using System.Globalization;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    /// <summary>
    /// Service to resolve a game's name
    /// </summary>
    public interface IGameNameResolver
    {
        /// <summary>
        /// Resolves a culture invariant name of the game.
        /// </summary>
        /// <param name="game">The game which name shall get resolved.</param>
        /// <returns>The resolved name.</returns>
        /// <exception cref="PetroglyphException">if no name could be resolved.</exception>
        string ResolveName(IGameIdentity game);

        /// <summary>
        /// Resolves a culture invariant name of the game.
        /// </summary>
        /// <param name="game">The game which name shall get resolved.</param>
        /// <param name="culture">The culture context.</param>
        /// <returns>The resolved name. May return <see langword="null"/> if no name name for the <paramref name="culture"/> could be found.</returns>
        string? ResolveName(IGameIdentity game, CultureInfo culture);
    }
}