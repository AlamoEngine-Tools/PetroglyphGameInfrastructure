using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Factory that creates client instances for <see cref="IGame"/>.
/// </summary>
public interface IGameClientFactory
{
    /// <summary>
    /// Creates a new client for the specified <paramref name="game"/>.
    /// </summary>
    /// <param name="game">The game to create a client for.</param>
    /// <returns>The <see cref="IGameClient"/> for <paramref name="game"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    IGameClient CreateClient(IGame game);
}