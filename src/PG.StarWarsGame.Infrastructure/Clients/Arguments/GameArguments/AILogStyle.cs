using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Level of detail of LUA logging.
/// </summary>
[SerializeEnumName]
public enum AILogStyle
{
    /// <summary>
    /// LUA logging is disabled.
    /// </summary>
    None,
    /// <summary>
    /// Default LUA logging level.
    /// </summary>
    Normal,
    /// <summary>
    /// Verbose LUA logging level.
    /// </summary>
    Heavy
}