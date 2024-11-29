using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// The exception that is thrown when a dependency version does not match the expected version range.
/// </summary>
public class VersionMismatchException : ModDependencyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionMismatchException"/> class of the specified source reference and dependency.
    /// </summary>
    /// <param name="source">The source reference which defines the version range.</param>
    /// <param name="dependency">The dependency with the mismatching version.</param>
    public VersionMismatchException(IModReference source, IMod dependency) : base(source, dependency)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionMismatchException"/> class of the specified source reference and dependency
    /// and with an error message.
    /// </summary>
    /// <param name="source">The source reference which defines the version range.</param>
    /// <param name="dependency">The dependency with the mismatching version.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public VersionMismatchException(IModReference source, IMod dependency, string message) : base(source, dependency, message)
    {
    }
}