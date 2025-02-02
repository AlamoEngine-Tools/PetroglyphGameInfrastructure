using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;

namespace AET.SteamAbstraction.Library;

/// <summary>
/// Represents a Steam Library location.
/// </summary>
public interface ISteamLibrary : IEquatable<ISteamLibrary>
{
    /// <summary>
    /// Gets the location of the library.
    /// </summary>
    IDirectoryInfo LibraryLocation { get; }

    /// <summary>
    /// Gets the 'steamapps' directory.
    /// </summary>
    IDirectoryInfo SteamAppsLocation { get; }

    /// <summary>
    /// Gets the 'common' directory of the <see cref="SteamAppsLocation"/>
    /// </summary>
    IDirectoryInfo CommonLocation { get; }

    /// <summary>
    /// Gets the 'workshop' directory of the <see cref="SteamAppsLocation"/>
    /// </summary>
    IDirectoryInfo WorkshopsLocation { get; }

    /// <summary>
    /// Gets the installed game manifests of this library.
    /// </summary>
    /// <returns>Collection of game manifests.</returns>
    IEnumerable<SteamAppManifest> GetApps();
}