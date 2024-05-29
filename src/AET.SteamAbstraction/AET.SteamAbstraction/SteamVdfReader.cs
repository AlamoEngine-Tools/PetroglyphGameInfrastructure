using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AnakinRaW.CommonUtilities.FileSystem;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal class SteamVdfReader : ISteamVdfReader
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

        if (manifestData.Key != "AppState")
            throw new VdfException("Invalid Data: Expected 'AppState' as root.");

        uint? id = null;
        string? name = null;
        SteamAppState? state = null;
        string? installDir = null;
        uint[]? depots = null;
        foreach (var child in manifestData.Value.Children<VProperty>())
        {
            switch (child.Key.ToLower())
            {
                case "appid":
                    id = uint.Parse(child.Value.ToString());
                    break;
                case "name":
                    name = child.Value.ToString();
                    break;
                case "stateflags":
                    state = (SteamAppState)int.Parse(child.Value.ToString());
                    break;
                case "installdir":
                    installDir = child.Value.ToString();
                    break;
                case "installeddepots":
                    if (child.Value is not VObject depotsObject)
                        break;
                    depots = new uint[depotsObject.Count];
                    var count = 0;
                    foreach (var depot in depotsObject.Children<VProperty>())
                        depots[count++] = uint.Parse(depot.Key);
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

        return new SteamAppManifest(library, manifestFile, id.Value, name!, installLocation, state.Value, new HashSet<uint>(depots));
    }

    public IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile)
    {
        var libraryData = ReadFileAsJson(configFile);

        if (libraryData.Key != "libraryfolders")
            throw new VdfException("Invalid Data: Expected 'libraryfolders' as root.");

        if (libraryData.Value is not VObject vLibs)
            throw new VdfException("Invalid Data: Expected 'libraryfolders' to contain objects.");


        foreach (var prop in vLibs.Children<VProperty>())
        {
            // Skipping everyChild which is not a number
            if (!int.TryParse(prop.Key, out _))
                continue;

            if (prop.Value is VValue pathValue)
                yield return _fileSystem.DirectoryInfo.New(pathValue.Value<string>());
            else if (prop.Value is VObject obj)
            {
                foreach (var childProperty in obj.Children<VProperty>())
                {
                    if (!childProperty.Key.Equals("path") || childProperty.Value is not VValue value)
                        continue;
                    yield return _fileSystem.DirectoryInfo.New(value.Value<string>());
                }
            }
        }
    }

    public LoginUsers ReadLoginUsers(IFileInfo configFile)
    {
        if (configFile == null)
            throw new ArgumentNullException(nameof(configFile));
        var loginData = ReadFileAsJson(configFile);

        
        if (loginData.Key != "users")
            throw new VdfException("Invalid Data: Expected 'users' as root.");

        if (loginData.Value is not VObject vUsers)
            throw new VdfException("Invalid Data: Expected 'users' to contain objects.");

        var users = new List<SteamUserLoginMetadata>(vUsers.Count);

        foreach (var user in vUsers.Children<VProperty>())
        {
            if (user.Value is not VObject userProps)
                throw new VdfException("Invalid Data: Expected user data to contain an object.");

            var wantsOffline = false;
            var mostRecent = false;
            foreach (var property in userProps.Children<VProperty>())
            {
                switch (property.Key)
                {
                    case "MostRecent":
                        mostRecent = ParseBooleanNumberString(property);
                        break;
                    case "WantsOfflineMode":
                        wantsOffline = ParseBooleanNumberString(property);
                        break;
                }
            }

            users.Add(new SteamUserLoginMetadata(mostRecent, wantsOffline));
        }

        return new LoginUsers(users);
    }

    private static bool ParseBooleanNumberString(VProperty property)
    {
        var value = ((VValue)property.Value).Value<string>();
        if (!int.TryParse(value, out var numValue))
            throw new VdfException($"Expected number string for property {property.Key}");
        return numValue switch
        {
            0 => false,
            1 => true,
            _ => throw new VdfException($"Expected number string for property {property.Key}")
        };
    }

    private static VProperty ReadFileAsJson(IFileInfo file)
    {
        try
        {
            return VdfConvert.Deserialize(file.OpenText());
        }
        catch (VdfException e)
        {
            throw new VdfException($"Failed reading {file.FullName}: {e.Message}");
        }
    }
}