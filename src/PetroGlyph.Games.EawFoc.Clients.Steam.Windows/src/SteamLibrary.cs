using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using Microsoft.Extensions.DependencyInjection;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal class SteamLibrary : ISteamLibrary
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<KnownLibraryLocations, IDirectoryInfo> _locations = new();

    private readonly Dictionary<KnownLibraryLocations, string[]> _locationsNames = new()
    {
        { KnownLibraryLocations.SteamApps, new[] { "steamapps" } },
        { KnownLibraryLocations.Common, new[] { "steamapps", "common" } },
        { KnownLibraryLocations.Workshops, new[] { "steamapps", "workshop" } }
    };

    private readonly string _normalizedLocation;
    private readonly IFileSystem _fileSystem;

    public IDirectoryInfo LibraryLocation { get; }

    public IDirectoryInfo SteamAppsLocation => GetKnownLibraryLocation(KnownLibraryLocations.SteamApps);

    public IDirectoryInfo CommonLocation => GetKnownLibraryLocation(KnownLibraryLocations.Common);

    public IDirectoryInfo WorkshopsLocation => GetKnownLibraryLocation(KnownLibraryLocations.Workshops);
    
    public SteamLibrary(IDirectoryInfo libraryLocation, IServiceProvider serviceProvider)
    {
        if (libraryLocation == null) 
            throw new ArgumentNullException(nameof(libraryLocation));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _normalizedLocation = PathNormalizer.Normalize(_fileSystem.Path.GetFullPath(libraryLocation.FullName), new PathNormalizeOptions
        {
            UnifyCase = UnifyCasingKind.UpperCase,
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim
        });
        LibraryLocation = libraryLocation;
    }

    public IEnumerable<SteamAppManifest> GetApps()
    {
        if (!SteamAppsLocation.Exists)
            return Array.Empty<SteamAppManifest>();
        var apps = new HashSet<SteamAppManifest>();
        var manifestReader = _serviceProvider.GetRequiredService<ISteamAppManifestReader>();
        foreach (var manifestFile in SteamAppsLocation.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly))
            apps.Add(manifestReader.ReadManifest(manifestFile, this));
        return apps;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        return obj is ISteamLibrary other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _normalizedLocation.GetHashCode();
    }

    public bool Equals(ISteamLibrary? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        var normalizedOtherPath = PathNormalizer.Normalize(_fileSystem.Path.GetFullPath(other.LibraryLocation.FullName),
            new PathNormalizeOptions
            {
                UnifyCase = UnifyCasingKind.UpperCase,
                TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim
            });

        return _normalizedLocation.Equals(normalizedOtherPath);
    }

    public override string ToString()
    {
        return $"SteamLibrary:'{LibraryLocation.FullName}'";
    }

    private IDirectoryInfo GetKnownLibraryLocation(KnownLibraryLocations location)
    {
        return _locations.GetOrAdd(location, l =>
        {
            var fs = LibraryLocation.FileSystem;
            var subPathParts = _locationsNames[l];
            var basePath = LibraryLocation.FullName;

            var pathParts = new string[subPathParts.Length + 1];
            pathParts[0] = basePath;
            Array.Copy(subPathParts, 0, pathParts, 1, subPathParts.Length);

            var locationPath = fs.Path.Combine(pathParts);
            return fs.DirectoryInfo.New(locationPath);
        });
    }

    private enum KnownLibraryLocations
    {
        SteamApps,
        Common,
        Workshops
    }
}