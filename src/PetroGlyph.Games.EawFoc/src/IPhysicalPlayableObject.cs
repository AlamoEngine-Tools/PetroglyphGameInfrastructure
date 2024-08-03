using System.IO;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// An <see cref="IPlayableObject"/> which has a file system and usually is installed on a machine.
/// </summary>
public interface IPhysicalPlayableObject : IPlayableObject
{
    /// <summary>
    /// Returns a <see cref="DirectoryInfo"/> of the root directory.
    /// </summary>
    IDirectoryInfo Directory { get; }

    /// <summary>
    /// The filesystem used by this instance.
    /// </summary>
    IFileSystem FileSystem { get; }
}