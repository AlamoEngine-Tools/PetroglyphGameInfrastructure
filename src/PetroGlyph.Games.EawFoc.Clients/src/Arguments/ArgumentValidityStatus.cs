namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Indicates the validity of a <see cref="IGameArgument"/>
/// </summary>
public enum ArgumentValidityStatus
{
    /// <summary>
    /// The argument is valid
    /// </summary>
    Valid,
    /// <summary>
    /// The argument has an invalid name.
    /// </summary>
    InvalidName,
    /// <summary>
    /// The argument contains an illegal character.
    /// </summary>
    IllegalCharacter,
    /// <summary>
    /// The argument contains a space character. This is not legal for e.g, filesystem paths.
    /// </summary>
    PathContainsSpaces,
    /// <summary>
    /// The argument was expected to have a value, but it has none or was empty.
    /// </summary>
    EmptyData,
    /// <summary>
    /// The argument has an invalid value.
    /// </summary>
    InvalidData
}