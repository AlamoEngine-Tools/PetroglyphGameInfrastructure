namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Flag indication the kindness of an <see cref="IGameArgument"/>
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
    /// Synthetic kind, to indicate this argument contains an ordered list of mod arguments.
    /// </summary>
    ModList
}