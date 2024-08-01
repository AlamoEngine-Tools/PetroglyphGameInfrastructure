namespace PG.StarWarsGame.Infrastructure.Games.Registry;

/// <summary>
/// The exception that is thrown when a <see cref="IGameRegistry"/> could not be found.
/// </summary>
public class GameRegistryNotFoundException : GameException
{
}