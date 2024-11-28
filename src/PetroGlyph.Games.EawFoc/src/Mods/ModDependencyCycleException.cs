using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// A <see cref="ModException"/> which gets thrown whenever a dependency cycle was detected.
/// </summary>
public sealed class ModDependencyCycleException : ModDependencyException
{
    /// <summary>
    /// Creates a new exception where <paramref name="mod"/> is the root mod of the requested dependency chain.
    /// </summary>
    /// <param name="mod">The root mod of the dependency chain.</param>
    public ModDependencyCycleException(IModReference mod) : base(mod, mod)
    {
    }

    /// <summary>
    /// Creates a new exception where <paramref name="mod"/> is the root mod of the requested dependency chain.
    /// </summary>
    /// <param name="mod">The root mod of the dependency chain.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ModDependencyCycleException(IModReference mod, string message) : base(mod, mod, message)
    {
    }
}