using System;

namespace PetroGlyph.Games.EawFoc.Games.Registry;

/// <summary>
/// Factory service to create an <see cref="IGameRegistry"/>
/// </summary>
public interface IGameRegistryFactory
{
    /// <summary>
    /// Returns a <see cref="IGameRegistry"/> for a requested <see cref="GameType"/>.
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the registry.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> for creating the registry.</param>
    /// <returns>An instance of the registry for the given <paramref name="type"/>.</returns>
    /// <exception cref="GameRegistryNotFoundException">The game's registry does not exist.</exception>
    IGameRegistry CreateRegistry(GameType type, IServiceProvider serviceProvider);
}