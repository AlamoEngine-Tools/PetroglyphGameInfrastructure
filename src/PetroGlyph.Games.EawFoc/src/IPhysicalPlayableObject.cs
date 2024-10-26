using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Represents a playable object which is stored to the file system and usually is installed on a machine.
/// </summary>
public interface IPhysicalPlayableObject : IPlayableObject
{
    /// <summary>
    /// Gets the directory of this object.
    /// </summary>
    IDirectoryInfo Directory { get; }
}