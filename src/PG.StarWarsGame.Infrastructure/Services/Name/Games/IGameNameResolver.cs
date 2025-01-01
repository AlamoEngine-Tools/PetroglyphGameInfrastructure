using System;
using System.Globalization;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Service to resolve a game's name
/// </summary>
public interface IGameNameResolver
{
    /// <summary>
    /// Resolves the name of specified game.
    /// </summary>
    /// <param name="game">The game to resolve the name for.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The resolved name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    string ResolveName(IGameIdentity game, CultureInfo culture);
}