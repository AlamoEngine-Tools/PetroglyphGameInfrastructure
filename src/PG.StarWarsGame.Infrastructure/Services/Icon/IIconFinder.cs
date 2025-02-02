using System;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Service to find icons for mods.
/// </summary>
public interface IIconFinder
{
    /// <summary>
    /// Search an icon for the specified playable object.
    /// </summary>
    /// <param name="playableObject">The playable object to search an icon for.</param>
    /// <returns>Path of the found icon or <see langword="null"/> if not icon was found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    string? FindIcon(IPlayableObject playableObject);
}