using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Represents a detected mods reference with the optional associated <see cref="IModinfo"/>.
/// </summary>
public sealed class DetectedModReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DetectedModReference"/> with the specified mod reference and modinfo.
    /// </summary>
    /// <param name="modReference">The detected mod reference.</param>
    /// <param name="modInfo">The optional detected modinfo.</param>
    public DetectedModReference(IModReference modReference, IModinfo? modInfo)
    {
        ModReference = modReference ?? throw new ArgumentNullException(nameof(modReference));
        ModInfo = modInfo;
    }

    /// <summary>
    /// Gets the detected mod reference.
    /// </summary>
    public IModReference ModReference { get; }

    /// <summary>
    /// Gets the detected modinfo or <see langword="null"/> if none was detected.
    /// </summary>
    public IModinfo? ModInfo { get; }
}