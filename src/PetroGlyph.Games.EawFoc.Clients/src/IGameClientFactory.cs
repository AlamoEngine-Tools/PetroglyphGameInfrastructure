using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

/// <summary>
/// Factory that allows the creation of different <see cref="IGameClient"/>s
/// based on the actual <see cref="GamePlatform"/> of a game.
/// </summary>
public interface IGameClientFactory
{
    /// <summary>
    /// Gets an instance of an <see cref="IGameClient"/> for the given <paramref name="gamePlatform"/>.
    /// </summary>
    /// <param name="gamePlatform">The requested <see cref="GamePlatform"/>.</param>
    /// <param name="serviceProvider">The service provider used to create new <see cref="IGameClient"/> instances.</param>
    /// <returns></returns>
    IGameClient CreateClient(GamePlatform gamePlatform, IServiceProvider serviceProvider);
}