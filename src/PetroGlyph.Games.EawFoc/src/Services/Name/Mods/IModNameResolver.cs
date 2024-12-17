using System;
using System.Globalization;
using EawModinfo.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Service to resolve a mod's name
/// </summary>
public interface IModNameResolver
{
    /// <summary>
    /// Resolves the name of the specified mod reference and culture.
    /// </summary>
    /// <param name="detectedMod">The mod which name shall get resolved.</param>
    /// <param name="culture">The culture context.</param>
    /// <returns>The resolved name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="detectedMod"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="detectedMod"/> contains a reference to a virtual mod.</exception>
    string ResolveName(DetectedModReference detectedMod, CultureInfo culture);
}