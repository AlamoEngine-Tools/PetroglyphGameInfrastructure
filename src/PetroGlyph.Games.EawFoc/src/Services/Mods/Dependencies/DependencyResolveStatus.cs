using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies;

/// <summary>
/// Indicates the resolve state of an <see cref="IMod"/>
/// </summary>
public enum DependencyResolveStatus
{
    /// <summary>
    /// Dependencies are not yet resolved.
    /// </summary>
    None,
    /// <summary>
    /// Dependency are currently resolved.
    /// </summary>
    Resolving,
    /// <summary>
    /// Dependencies have been successfully resolved.
    /// </summary>
    Resolved,
    /// <summary>
    /// The last resolve operation was not successful.
    /// </summary>
    Faulted
}