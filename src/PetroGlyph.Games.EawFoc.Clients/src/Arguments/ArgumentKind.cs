namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Indicates the kindness of an <see cref="GameArgument"/>.
/// </summary>
public enum ArgumentKind
{
    /// <summary>
    /// The argument has flag semantics. It only needs to exist on the command line.
    /// </summary>
    Flag,
    /// <summary>
    /// The argument has flag semantics. It only needs to exist on the command line, but with a prefixed '-'.
    /// </summary>
    DashedFlag,
    /// <summary>
    /// The argument has custom data.
    /// </summary>
    KeyValue,
    /// <summary>
    /// Synthetic argument, which contains an ordered list of mod arguments.
    /// </summary>
    ModList
}