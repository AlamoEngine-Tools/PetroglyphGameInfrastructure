namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Minimal information to identify and distinguish Petroglyph Star Wars games from each other.
/// </summary>
public interface IGameIdentity
{
    /// <summary>
    /// The type of the game.
    /// </summary>
    public GameType Type { get; }

    /// <summary>
    /// The platform of the game.
    /// </summary>
    public GamePlatform Platform { get; }
}