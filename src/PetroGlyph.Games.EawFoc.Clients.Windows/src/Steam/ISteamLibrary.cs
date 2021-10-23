using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public interface ISteamLibrary : IEquatable<ISteamLibrary>
{
    IDirectoryInfo LibraryLocation { get; }

    IDirectoryInfo SteamAppsLocation { get; }

    IDirectoryInfo CommonLocation { get; }

    IDirectoryInfo WorkshopsLocation { get; }

    IEnumerable<SteamAppManifest> GetApps();
}