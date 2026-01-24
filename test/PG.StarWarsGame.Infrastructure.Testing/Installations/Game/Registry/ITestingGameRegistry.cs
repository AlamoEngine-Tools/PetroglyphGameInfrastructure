// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

/// <summary>
/// Represents an abstraction for a test registry that provides access to the actual <see cref="IGameRegistry"/>.
/// </summary>
public interface ITestingGameRegistry
{
    /// <summary>
    /// Creates a game registry instance for a game type that does not exist in the registry.
    /// </summary>
    /// <param name="gameType">The type of the game for which the registry should be created.</param>
    /// <returns>An instance of <see cref="IGameRegistry"/> representing a non-existing game registry.</returns>
    IGameRegistry CreateNonExistingRegistry(GameType gameType);

    /// <summary>
    /// Creates a registry for a game that is considered installed.
    /// </summary>
    /// <param name="game">The game for which the registry is to be created.</param>
    /// <returns>An instance of <see cref="IGameRegistry"/> representing the installed game registry.</returns>
    IGameRegistry CreateInstalled(IGame game);

    /// <summary>
    /// Creates an instance of <see cref="IGameRegistry"/> based on the provided setup data.
    /// </summary>
    /// <param name="registrySetupData">The setup data used to configure the game registry.</param>
    /// <returns>
    /// An instance of <see cref="IGameRegistry"/> configured according to the provided setup data.
    /// </returns>
    /// <remarks>
    /// This method is intended for testing purposes and allows for the creation of a game registry
    /// that simulates various states, such as installed or uninitialized.
    /// </remarks>
    IGameRegistry CreateFrom(TestGameRegistrySetupData registrySetupData);
}