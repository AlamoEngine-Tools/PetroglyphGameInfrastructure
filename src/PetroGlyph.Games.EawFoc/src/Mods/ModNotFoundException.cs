using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// Exception indicating a queried mod, <see cref="IMod"/> or <see cref="IModReference"/> was not found in some <see cref="IModContainer"/> instance.
/// </summary>
public class ModNotFoundException : PetroglyphException
{
    private readonly IModReference _modReference;
    private readonly IModContainer _modContainer;

    /// <inheritdoc/>
    public override string Message =>
        $"Unable to find mod '{_modReference.Identifier}' from {_modContainer}";

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/>which could not be found.</param>
    /// <param name="modContainer">The container which was queried.</param>
    public ModNotFoundException(IModReference modReference, IModContainer modContainer)
    {
        _modReference = modReference;
        _modContainer = modContainer;
    }
}