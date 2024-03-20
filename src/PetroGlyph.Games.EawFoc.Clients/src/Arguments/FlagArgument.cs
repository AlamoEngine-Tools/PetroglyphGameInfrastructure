using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Argument which enables a game behavior because it exists.
/// </summary>
/// <remarks>This argument can be explicitly unset by setting the value to <see langword="false"/>.
/// On the command line it will just omitted.</remarks>
public abstract class FlagArgument : GameArgument<bool>
{
    /// <summary>
    /// Can either be <see cref="ArgumentKind.Flag"/> or <see cref="ArgumentKind.DashedFlag"/>.
    /// </summary>
    public sealed override ArgumentKind Kind { get; }

    /// <inheritdoc/>
    public override string Name { get; }

    /// <summary>
    /// Initializes a new argument with a given value and the inforation if this argument is for debug mode only.
    /// </summary>
    /// <param name="name">The name of the flag.</param>
    /// <param name="value">Whether this flag is enabled or disabled.</param>
    /// <param name="dashed">When <see langword="true"/> this argument requires a dash '-' on the command line.</param>
    /// <param name="debug">Indicates whether this instance if for debug mode only.</param>
    protected FlagArgument(string name, bool value, bool dashed = false, bool debug = false) : base(value, debug)
    {
        Name = name;
        Kind = dashed ? ArgumentKind.DashedFlag : ArgumentKind.Flag;
    }

    /// <inheritdoc/>
    public sealed override string ValueToCommandLine()
    {
        return new ArgumentValueSerializer().Serialize(Value);
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