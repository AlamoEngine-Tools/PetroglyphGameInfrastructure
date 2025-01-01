using System;
using System.Diagnostics.CodeAnalysis;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <inheritdoc cref="IGameIdentity"/>
public sealed class GameIdentity(GameType type, GamePlatform platform) : IGameIdentity, IEquatable<GameIdentity>
{
    /// <inheritdoc />
    public GameType Type { get; } = type;

    /// <inheritdoc />
    public GamePlatform Platform { get; } = platform;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as GameIdentity);
    }

    /// <inheritdoc />
    public bool Equals(GameIdentity? other)
    {
        if (other is null) 
            return false;
        if (ReferenceEquals(this, other)) 
            return true;
        return ((IGameIdentity)this).Equals(other);
    }

    /// <inheritdoc />
    bool IEquatable<IGameIdentity>.Equals(IGameIdentity? other)
    {
        return Type == other?.Type && Platform == other.Platform;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, (int)Platform);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{Type}:{Platform}";
    }
}