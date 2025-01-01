using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Argument which enables a game behavior.
/// </summary>
/// <remarks>
/// This argument can be explicitly unset by setting the value to <see langword="false"/>.
/// In this case the argument is omitted on the command line.
/// </remarks>
public abstract class FlagArgument : GameArgument<bool>
{
    /// <summary>
    /// Gets a value indicating whether the flag argument is used with a prepended dash ('-') character
    /// </summary>
    public bool Dashed { get; }

    /// <summary>
    /// Initializes a new argument with a given value and the inforation if this argument is for debug mode only.
    /// </summary>
    /// <param name="name">The name of the flag.</param>
    /// <param name="value">Whether this flag is enabled or disabled.</param>
    /// <param name="dashed">When <see langword="true"/> this argument requires a dash '-' on the command line.</param>
    /// <param name="debug">Indicates whether this instance if for debug mode only.</param>
    private protected FlagArgument(string name, bool value, bool dashed = false, bool debug = false) : base(name, value, debug)
    {
        Dashed = dashed;
    }

    private protected override bool EqualsValue(GameArgument other)
    {
        if (other is not FlagArgument otherFlag)
            return false;
        return Value.Equals(other.Value) && Dashed == otherFlag.Dashed;
    }

    private protected override int ValueHash()
    {
        return HashCode.Combine(Value, Dashed);
    }
}