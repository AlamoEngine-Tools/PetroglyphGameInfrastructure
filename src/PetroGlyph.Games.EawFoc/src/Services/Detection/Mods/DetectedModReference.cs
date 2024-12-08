using System;
using System.IO.Abstractions;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Represents a detected mod reference with the optional associated <see cref="IModinfo"/>.
/// </summary>
public sealed class DetectedModReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DetectedModReference"/> with the specified mod reference and modinfo.
    /// </summary>
    /// <param name="modReference">The detected mod reference.</param>
    /// <param name="directory">The directory of the mod.</param>
    /// <param name="modInfo">The optional detected modinfo.</param>
    /// <exception cref="ArgumentNullException"><paramref name="modReference"/> is <see langword="null"/>.</exception>
    public DetectedModReference(IModReference modReference, IDirectoryInfo directory, IModinfo? modInfo)
    {
        ModReference = modReference ?? throw new ArgumentNullException(nameof(modReference));
        ModInfo = modInfo;
        Directory = directory;
    }

    /// <summary>
    /// Gets the detected mod reference.
    /// </summary>
    public IModReference ModReference { get; }

    /// <summary>
    /// Gets the detected directory of the mod.
    /// </summary>
    /// <remarks>The value can be <see langword="null"/>, if the detected mod reference is a virtual mod.</remarks>
    public IDirectoryInfo? Directory { get; }

    /// <summary>
    /// Gets the detected modinfo or <see langword="null"/> if none was detected.
    /// </summary>
    public IModinfo? ModInfo { get; }
}