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
    /// Can either be <see cref="ArgumentKind.Flag"/> or <see cref="ArgumentKind.DashedFlag"/>.
    /// </summary>
    public sealed override ArgumentKind Kind { get; }

    /// <summary>
    /// Initializes a new argument with a given value and the inforation if this argument is for debug mode only.
    /// </summary>
    /// <param name="name">The name of the flag.</param>
    /// <param name="value">Whether this flag is enabled or disabled.</param>
    /// <param name="dashed">When <see langword="true"/> this argument requires a dash '-' on the command line.</param>
    /// <param name="debug">Indicates whether this instance if for debug mode only.</param>
    private protected FlagArgument(string name, bool value, bool dashed = false, bool debug = false) : base(name, value, debug)
    {
        Kind = dashed ? ArgumentKind.DashedFlag : ArgumentKind.Flag;
    }

    private protected override bool EqualsValue(GameArgument other)
    {
        return Value.Equals(other.Value);
    }

    private protected override int ValueHash()
    {
        return Value.GetHashCode();
    }
}