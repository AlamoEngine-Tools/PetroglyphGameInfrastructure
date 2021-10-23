namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// A <see cref="ModException"/> which gets thrown whenever a dependency cycle was detected.
/// </summary>
public class ModDependencyCycleException : ModException
{
    /// <inheritdoc/>
    public ModDependencyCycleException()
    {
    }

    /// <inheritdoc/>
    public ModDependencyCycleException(string message) : base(message)
    {
    }
}