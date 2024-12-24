using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.SteamAbstraction.Library;

internal sealed class SteamLibraryFinder(IServiceProvider serviceProvider) : ISteamLibraryFinder
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ISteamRegistryFactory _registryFactory = serviceProvider.GetRequiredService<ISteamRegistryFactory>();
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SteamLibraryFinder));

    public IEnumerable<ISteamLibrary> FindLibraries()
    {
        _logger?.LogTrace("Searching for Steam libraries on system");
        var libraryLocationsFile = GetLibraryLocationsFile();

        if (!libraryLocationsFile.Exists)
        {
            _logger?.LogWarning("Config file with Steam libraries not found. No Steam libraries are created.");
            return Array.Empty<ISteamLibrary>();
        }

        var libraryLocations = SteamVdfReader.ReadLibraryLocationsFromConfig(libraryLocationsFile);
        var libraries = new HashSet<ISteamLibrary>();
        foreach (var libraryLocation in libraryLocations)
        {
            if (TryCreateLibraryFromLocation(libraryLocation, out var library))
                libraries.Add(library);
        }
        return libraries;
    }

    private bool TryCreateLibraryFromLocation(IDirectoryInfo libraryLocation, [NotNullWhen(true)] out SteamLibrary? library)
    {
        _logger?.LogTrace($"Try creating steam library from location '{libraryLocation.FullName}'");
        library = null;

        if (!libraryLocation.Exists)
        {
            _logger?.LogTrace($"Steam library location '{libraryLocation.FullName}' does not exist. Library not created.");
            return false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var steamDll = _fileSystem.Path.Combine(libraryLocation.FullName, "steam.dll");
            if (!_fileSystem.File.Exists(steamDll))
            {
                _logger?.LogTrace($"Steam library location '{libraryLocation.FullName}' does not contain 'steam.dll'. Library not created.");
                return false;
            }
        }

        var libraryVdf = _fileSystem.Path.Combine(libraryLocation.FullName, "libraryfolder.vdf");
        if (!_fileSystem.File.Exists(libraryVdf))
        {
            _logger?.LogTrace($"Steam library location '{libraryLocation.FullName}' does not contain 'libraryfolder.vdf'. Library not created.");
            return false;
        }

        library = new SteamLibrary(libraryLocation, _serviceProvider);
        return true;
    }

    private IFileInfo GetLibraryLocationsFile()
    {
        using var registry = _registryFactory.CreateRegistry();
        var steamInstallLocation = registry.InstallationDirectory;
        if (steamInstallLocation is null)
        {
            var e = new SteamException("Unable to find an installation of Steam.");
            _logger?.LogError(e, "Unable to find Steam installation from registry");
            throw e;
        }

        return _fileSystem.FileInfo.New(_fileSystem.Path.Combine(steamInstallLocation.FullName, "config", "libraryfolders.vdf"));
    }
}