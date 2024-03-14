using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal class SteamVdfReader : ISteamAppManifestReader, ILibraryConfigReader
{
    private readonly IFileSystem _fileSystem;

    public SteamVdfReader(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library)
    {
        if (manifestFile == null) 
            throw new ArgumentNullException(nameof(manifestFile));
        if (library == null) 
            throw new ArgumentNullException(nameof(library));

        var directory = manifestFile.Directory;
        if (directory is null || !_fileSystem.Path.IsChildOf(library.LibraryLocation.FullName, directory.FullName))
            throw new SteamException("The game's manifest is not part of the given library");
        var manifestData = ReadFileAsJson(manifestFile);
        if (manifestData.Name != "AppState")
            throw new SteamException("Invalid Data: Expected 'AppState' as root.");

        uint? id = null;
        string? name = null;
        SteamAppState? state = null;
        string? installDir = null;
        uint[]? depots = null;
        foreach (var child in manifestData.Value.Children())
        {
            if (child is not JProperty property)
                continue;
            switch (property.Name.ToLower())
            {
                case "appid":
                    id = uint.Parse(property.Value.ToString());
                    break;
                case "name":
                    name = property.Value.ToString();
                    break;
                case "stateflags":
                    state = (SteamAppState)int.Parse(property.Value.ToString());
                    break;
                case "installdir":
                    installDir = property.Value.ToString();
                    break;
                case "installeddepots":
                    var depotsObject = property.Value as JObject;
                    if (depotsObject is null)
                        break;
                    depots = new uint[depotsObject.Count];
                    var count = 0;
                    foreach (var depot in depotsObject.Children())
                        depots[count++] = uint.Parse(((JProperty)depot).Name);
                    break;
                default: 
                    continue;
            }
        }

        if (id is null ||
            string.IsNullOrEmpty(name) ||
            state is null ||
            string.IsNullOrEmpty(installDir) ||
            depots is null)
            throw new SteamException($"Invalid App Manifest at file {manifestFile.FullName}");

        var installLocation = _fileSystem.DirectoryInfo.New(
            _fileSystem.Path.Combine(library.CommonLocation.FullName, installDir!));

        return new SteamAppManifest(library, manifestFile, id.Value, name!, installLocation, state.Value,
            depots.ToHashSet());
    }

    public IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile)
    {
        var libraryData = ReadFileAsJson(configFile);
        if (libraryData.Name != "libraryfolders")
            throw new SteamException("Invalid Data: Expected 'libraryfolders' as root.");
        var paths = new HashSet<string>();
        foreach (var childNode in libraryData.Value.Children())
        {
            // Skipping everyChild which is not a number
            if (childNode is not JProperty jProp || !int.TryParse(jProp.Name, out _))
                continue;
            foreach (var childProperty in childNode.Values())
            {
                if (childProperty is JProperty property)
                {
                    if (!property.Name.Equals("path") || property.Value is not JValue { Type: JTokenType.String } pathValue)
                        continue;
                    paths.Add((string)pathValue.Value!);
                    break;
                }

                if (childProperty is not JValue { Type: JTokenType.String } childValue)
                    continue;
                paths.Add((string)childValue.Value!);
            }
        }
        return paths.Select(p => _fileSystem.DirectoryInfo.New(p));
    }

    private static JProperty ReadFileAsJson(IFileInfo file)
    {
        try
        {
            return VdfConvert.Deserialize(file.OpenText()).ToJson();
        }
        catch (VdfException e)
        {
            throw new SteamException($"Failed reading {file.FullName}: {e.Message}", e);
        }
    }
}