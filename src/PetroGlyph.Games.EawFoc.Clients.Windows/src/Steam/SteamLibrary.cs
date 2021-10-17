using System.Collections.Generic;
using System.IO.Abstractions;
using Sklavenwalker.CommonUtilities.FileSystem;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public class SteamLibrary
{
    private readonly string _normalizedLocation;

    public IDirectoryInfo LibraryLocation { get; }

    public ISet<SteamAppManifest> Apps { get; }

    public SteamLibrary(IDirectoryInfo libraryLocation, ISet<SteamAppManifest> apps)
    {
        LibraryLocation = libraryLocation;
        Apps = apps;
        var pathHelper = new PathHelperService(libraryLocation.FileSystem);
        _normalizedLocation = pathHelper.NormalizePath(libraryLocation.FullName, PathNormalizeOptions.Full);
    }

    public bool Equals(SteamLibrary? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return _normalizedLocation.Equals(other._normalizedLocation) && Apps.Equals(other.Apps);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        return obj is SteamLibrary other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (_normalizedLocation.GetHashCode() * 397) ^ Apps.GetHashCode();
        }
    }


    public override string ToString()
    {
        return $"SteamLibrary:'{LibraryLocation.FullName}'";
    }
}