using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.SteamAbstraction.Library;

internal class SteamLibrary : ISteamLibrary
{
    private static readonly PathNormalizeOptions SteamLibraryPathNormalizeOptions = new()
    {
        UnifyCase = UnifyCasingKind.UpperCase,
        TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim
    };

    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<KnownLibraryLocations, IDirectoryInfo> _locations = new();

    private readonly Dictionary<KnownLibraryLocations, string[]> _locationsNames = new()
    {
        { KnownLibraryLocations.SteamApps, ["steamapps"] },
        { KnownLibraryLocations.Common, ["steamapps", "common"] },
        { KnownLibraryLocations.Workshops, ["steamapps", "workshop"] }
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
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _normalizedLocation = PathNormalizer.Normalize(_fileSystem.Path.GetFullPath(libraryLocation.FullName), SteamLibraryPathNormalizeOptions);
        LibraryLocation = libraryLocation;
    }

    public IEnumerable<SteamAppManifest> GetApps()
    {
        if (!SteamAppsLocation.Exists)
            return [];
        var apps = new HashSet<SteamAppManifest>();
        foreach (var manifestFile in SteamAppsLocation.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var manifest = SteamVdfReader.ReadManifest(manifestFile, this);
                apps.Add(manifest);
            }
            catch (SteamException e)
            {
                _logger?.LogWarning(e, $"Could not read game manifest file '{manifestFile}': {e.Message}");
            }
        }
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

        var normalizedOtherPath = PathNormalizer.Normalize(_fileSystem.Path.GetFullPath(other.LibraryLocation.FullName), SteamLibraryPathNormalizeOptions);
        return _normalizedLocation.Equals(normalizedOtherPath);
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"SteamLibrary: '{LibraryLocation.FullName}'";
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