using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Library;

/// <summary>
/// Represents a Steam Library location.
/// </summary>
public interface ISteamLibrary : IEquatable<ISteamLibrary>
{
    /// <summary>
    /// Physical location of the library.
    /// </summary>
    IDirectoryInfo LibraryLocation { get; }

    /// <summary>
    /// The 'steamapps' directory.
    /// </summary>
    IDirectoryInfo SteamAppsLocation { get; }

    /// <summary>
    /// The 'common' directory of the <see cref="SteamAppsLocation"/>
    /// </summary>
    IDirectoryInfo CommonLocation { get; }

    /// <summary>
    /// The 'workshop' directory of the <see cref="SteamAppsLocation"/>
    /// </summary>
    IDirectoryInfo WorkshopsLocation { get; }

    /// <summary>
    /// The installed game manifests of this library.
    /// </summary>
    /// <returns>Collection of game manifests.</returns>
    IEnumerable<SteamAppManifest> GetApps();
}