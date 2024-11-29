using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// The exception that is thrown when anything related to mod dependencies failed.
/// </summary>
public class ModDependencyException : ModException
{
    /// <summary>
    /// Gets the affected dependency.
    /// </summary>
    public IModReference Dependency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModDependencyException"/> of the specified source and dependency.
    /// </summary>
    /// <param name="source">The dependency's source.</param>
    /// <param name="dependency">The dependency.</param>
    public ModDependencyException(IModReference source, IModReference dependency) : base(source)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModDependencyException"/> of the specified source and dependency
    /// with an error message.
    /// </summary>
    /// <param name="source">The dependency's source.</param>
    /// <param name="dependency">The dependency.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ModDependencyException(IModReference source, IModReference dependency, string message)
        : base(source, message)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModDependencyException"/> of the specified source and dependency
    /// with an error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="source">The dependency's source.</param>
    /// <param name="dependency">The dependency.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ModDependencyException(IModReference source, IModReference dependency, string message, Exception exception)
        : base(source, message, exception)
    {
        Dependency = dependency;
    }
}