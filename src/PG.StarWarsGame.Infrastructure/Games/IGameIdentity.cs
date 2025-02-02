using System;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Represents the minimal information to identify and distinguish different Petroglyph Star Wars games from each other.
/// </summary>
public interface IGameIdentity : IEquatable<IGameIdentity>
{
    /// <summary>
    /// Gets the type of the game.
    /// </summary>
    public GameType Type { get; }

    /// <summary>
    /// Gets the platform of the game.
    /// </summary>
    public GamePlatform Platform { get; }
}