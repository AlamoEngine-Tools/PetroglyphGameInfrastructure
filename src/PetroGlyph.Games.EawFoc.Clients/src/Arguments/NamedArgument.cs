using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// <see cref="IGameArgument"/> with the format Name=Value
/// </summary>
/// <remarks>
/// The value must not contain any space characters, as the game cannot handle them,
/// even when escaped or encapsulated with ' or "
/// </remarks>
/// <typeparam name="T">Type of the Value.</typeparam>
public abstract class NamedArgument<T> : GameArgument<T> where T : notnull
{
    /// <summary>
    /// Identifies this instance as <see cref="ArgumentKind.KeyValue"/>.
    /// </summary>
    public override ArgumentKind Kind => ArgumentKind.KeyValue;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <summary>
    /// Initializes a new <see cref="NamedArgument{T}"/> with a given name value.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <param name="isDebug">Indicates whether this argument is available in debug mode only.</param>
    protected NamedArgument(string name, T value, bool isDebug) : base(value, isDebug)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public override bool Equals(IGameArgument? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Kind == other.Kind && Name == other.Name && Value.Equals(other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is IGameArgument other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Name, Value);
    }
}