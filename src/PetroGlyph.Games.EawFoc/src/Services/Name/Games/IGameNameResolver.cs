using System.Globalization;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Service to resolve a game's name
/// </summary>
public interface IGameNameResolver
{
    /// <summary>
    /// Resolves a culture invariant name of the game.
    /// </summary>
    /// <param name="game">The game which name shall get resolved.</param>
    /// <param name="culture">The culture context.</param>
    /// <returns>The resolved name.</returns>
    string ResolveName(IGameIdentity game, CultureInfo culture);
}