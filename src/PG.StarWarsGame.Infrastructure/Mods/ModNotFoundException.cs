using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// The exception that is thrown when a mod or mod reference could not be found.
/// </summary>
public class ModNotFoundException : ModException
{
    /// <summary>
    /// Gets the mod container from which it was unable to find the mod reference.
    /// </summary>
    public IModContainer ModContainer { get; }

    /// <inheritdoc/>
    public override string Message => $"Unable to find mod '{Mod.Identifier}' from container '{ModContainer}'.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ModNotFoundException"/> class
    /// of the specified mod container and the mod reference which was not found.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> which could not be found.</param>
    /// <param name="modContainer">The container which was queried.</param>
    public ModNotFoundException(IModReference modReference, IModContainer modContainer) : base(modReference)
    {
        ModContainer = modContainer;
    }
}