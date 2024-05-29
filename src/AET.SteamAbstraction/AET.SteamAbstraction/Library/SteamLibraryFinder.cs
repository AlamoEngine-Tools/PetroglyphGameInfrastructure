using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction.Library;

internal class SteamLibraryFinder(IServiceProvider serviceProvider) : DisposableObject, ISteamLibraryFinder
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ISteamRegistry _registry = serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ISteamVdfReader _configReader = serviceProvider.GetRequiredService<ISteamVdfReader>();

    public IEnumerable<ISteamLibrary> FindLibraries()
    {
        var libraryLocationsFile = GetLibraryLocationsFile();
        if (libraryLocationsFile is null)
            return Array.Empty<ISteamLibrary>();
        var libraryLocations = _configReader.ReadLibraryLocationsFromConfig(libraryLocationsFile);
        var libraries = new HashSet<ISteamLibrary>();
        foreach (var libraryLocation in libraryLocations)
        {
            if (TryCreateLibraryFromLocation(libraryLocation, out var library))
                libraries.Add(library!);
        }
        return libraries;
    }

    private bool TryCreateLibraryFromLocation(IDirectoryInfo libraryLocation, out SteamLibrary? library)
    {
        library = null;
        var steamDll = _fileSystem.Path.Combine(libraryLocation.FullName, "steam.dll");
        if (!_fileSystem.File.Exists(steamDll))
            return false;
        
        library = new SteamLibrary(libraryLocation, _serviceProvider);
        return true;
    }

    private IFileInfo? GetLibraryLocationsFile()
    {
        var steamInstallLocation = _registry.InstallationDirectory;
        if (steamInstallLocation is null)
            throw new SteamException("Unable to find an installation of Steam.");

        var libraryLocationsFile = _fileSystem.FileInfo.New(
            _fileSystem.Path.Combine(steamInstallLocation.FullName, "steamapps/libraryfolders.vdf"));

        if (libraryLocationsFile.Exists)
            return libraryLocationsFile;

        libraryLocationsFile = _fileSystem.FileInfo.New(
            _fileSystem.Path.Combine(steamInstallLocation.FullName, "config/libraryfolders.vdf"));

        return libraryLocationsFile.Exists ? libraryLocationsFile : null;
    }

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        _registry.Dispose();
    }
}