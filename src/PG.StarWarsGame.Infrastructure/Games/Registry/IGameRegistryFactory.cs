namespace PG.StarWarsGame.Infrastructure.Games.Registry;

/// <summary>
/// A factory to create new instances of an <see cref="IGameRegistry"/>.
/// </summary>
public interface IGameRegistryFactory
{
    /// <summary>
    /// Create a new instance of an <see cref="IGameRegistry"/> for a specified <see cref="GameType"/>.
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the registry.</param>
    /// <returns>A new instance of the registry for the specified <paramref name="type"/>.</returns>
    IGameRegistry CreateRegistry(GameType type);
}