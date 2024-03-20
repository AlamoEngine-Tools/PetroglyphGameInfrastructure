using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// A <see cref="ModException"/> which gets thrown whenever a dependency cycle was detected.
/// </summary>
public class ModDependencyCycleException : ModException
{
    /// <summary>
    /// Creates a new exception where <paramref name="rootMod"/> is the root mod of the requested dependency chain.
    /// </summary>
    /// <param name="rootMod">The root mod of the dependency chain.</param>
    public ModDependencyCycleException(IModReference rootMod) : base(rootMod)
    {
    }

    /// <summary>
    /// Creates a new exception where <paramref name="rootMod"/> is the root mod of the requested dependency chain.
    /// </summary>
    /// <param name="rootMod">The root mod of the dependency chain.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ModDependencyCycleException(IModReference rootMod, string message) : base(rootMod, message)
    {
    }
}