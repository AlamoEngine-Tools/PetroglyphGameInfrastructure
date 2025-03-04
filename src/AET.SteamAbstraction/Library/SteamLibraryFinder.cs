using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.SteamAbstraction.Library;

internal sealed class SteamLibraryFinder(IServiceProvider serviceProvider) : ISteamLibraryFinder
{
    private const string SteamDllFileName = "steam.dll";
    private const string LibraryFolderVfdFileName = "libraryfolder.vdf";
    private const string LibraryFoldersVfdFileName = "libraryfolders.vdf";
    private const string ConfigFolderName = "config";
    private const string SteamAppsFolderName = "steamapps";

    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SteamLibraryFinder));

    public IEnumerable<ISteamLibrary> FindLibraries(IDirectoryInfo steamInstallDir)
    {
        _logger?.LogTrace("Searching for Steam libraries on system...");
        var libraryLocationsFile = GetLibraryLocationsFile(steamInstallDir);

        if (!libraryLocationsFile.Exists)
        {
            _logger?.LogTrace("Config file that should contain Steam libraries information is not found.");
            return Array.Empty<ISteamLibrary>();
        }

        var libraryLocations = SteamVdfReader.ReadLibraryLocationsFromConfig(libraryLocationsFile);
        var libraries = new HashSet<ISteamLibrary>();
        foreach (var libraryLocation in libraryLocations)
        {
            var isDefault = _fileSystem.Path.AreEqual(libraryLocation.FullName, steamInstallDir.FullName);
            if (TryCreateLibraryFromLocation(libraryLocation, isDefault, out var library))
                libraries.Add(library);
        }
        return libraries;
    }

    private bool TryCreateLibraryFromLocation(IDirectoryInfo libraryLocation, bool isDefault, [NotNullWhen(true)] out SteamLibrary? library)
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
            var steamDll = _fileSystem.Path.Combine(libraryLocation.FullName, SteamDllFileName);
            if (!_fileSystem.File.Exists(steamDll))
            {
                _logger?.LogTrace($"Steam library location '{libraryLocation.FullName}' does not contain 'steam.dll'. Library not created.");
                return false;
            }
        }

        var libraryVdfName = LibraryFolderVfdFileName;
        var libraryVdfSubPath = string.Empty;
        if (isDefault)
        {
            libraryVdfName = LibraryFoldersVfdFileName;
            libraryVdfSubPath = SteamAppsFolderName;
        }

        var libraryVdf = _fileSystem.Path.Combine(libraryLocation.FullName, libraryVdfSubPath, libraryVdfName);
        if (!_fileSystem.File.Exists(libraryVdf))
        {
            _logger?.LogTrace($"Steam library VDF file '{libraryVdf}' was not found. Library not created.");
            return false;
        }

        library = new SteamLibrary(libraryLocation, _serviceProvider);
        return true;
    }

    private IFileInfo GetLibraryLocationsFile(IDirectoryInfo steamInstallDir)
    {
        return _fileSystem.FileInfo.New(_fileSystem.Path.Combine(steamInstallDir.FullName, ConfigFolderName, LibraryFoldersVfdFileName));
    }
}