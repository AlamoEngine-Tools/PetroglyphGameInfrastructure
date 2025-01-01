using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Vdf;
using AET.SteamAbstraction.Vdf.Linq;
using AET.SteamAbstraction.Vdf.Utilities;
using AnakinRaW.CommonUtilities.FileSystem;

namespace AET.SteamAbstraction;

internal static class SteamVdfReader
{
    public static SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library)
    {
        if (manifestFile == null) 
            throw new ArgumentNullException(nameof(manifestFile));
        if (library == null) 
            throw new ArgumentNullException(nameof(library));

        var fs = manifestFile.FileSystem;

        if (manifestFile.Directory is null || !fs.Path.AreEqual(library.SteamAppsLocation.FullName, manifestFile.Directory.FullName))
            throw new SteamException("The game's manifest is not part of the given library");

        var manifestData = ParseFile(manifestFile);

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
                    if (uint.TryParse(child.Value.ToString(), NumberStyles.None, null, out var parsedId)) 
                        id = parsedId;
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

        var installLocation = fs.DirectoryInfo.New(
            fs.Path.Combine(library.CommonLocation.FullName, installDir!));

        return new SteamAppManifest(library, manifestFile, id.Value, name!, installLocation, state.Value, new HashSet<uint>(depots));
    }

    public static IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile)
    {
        var fs = configFile.FileSystem;

        var libraryData = ParseFile(configFile);

        if (libraryData.Key != "libraryfolders")
            throw new VdfException("Invalid Data: Expected 'libraryfolders' as root.");

        if (libraryData.Value is not VObject vLibs)
            throw new VdfException("Invalid Data: Expected 'libraryfolders' to contain objects.");


        foreach (var prop in vLibs.Children<VProperty>())
        {
            // Skipping everyChild which is not a number
            if (!int.TryParse(prop.Key, NumberStyles.None, null, out _))
                continue;

            if (prop.Value is VValue pathValue)
                yield return fs.DirectoryInfo.New(pathValue.Value<string>()!);
            else if (prop.Value is VObject obj)
            {
                foreach (var childProperty in obj.Children<VProperty>())
                {
                    if (!childProperty.Key.Equals("path") || childProperty.Value is not VValue value)
                        continue;
                    yield return fs.DirectoryInfo.New(value.Value<string>()!);
                }
            }
        }
    }

    public static LoginUsers ReadLoginUsers(IFileInfo configFile)
    {
        if (configFile == null)
            throw new ArgumentNullException(nameof(configFile));
       
        var loginData = ParseFile(configFile);

        if (loginData.Key != "users")
            throw new VdfException("Invalid Data: Expected 'users' as root.");

        if (loginData.Value is not VObject vUsers)
            throw new VdfException("Invalid Data: Expected 'users' to contain objects.");

        var users = new List<SteamUserLoginMetadata>(vUsers.Count);

        foreach (var user in vUsers.Children<VProperty>())
        {
            if (user.Value is not VObject userProps)
                throw new VdfException("Invalid Data: Expected user data to contain an object.");

            if (!ulong.TryParse(user.Key, NumberStyles.None, null, out var userId))
                continue;
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

            users.Add(new SteamUserLoginMetadata(userId, mostRecent, wantsOffline));
        }

        return new LoginUsers(users);
    }

    private static bool ParseBooleanNumberString(VProperty property)
    {
        var value = ((VValue)property.Value).Value<string>();
        if (!int.TryParse(value, out var numValue))
            throw new VdfException($"Expected number string for property {property.Key} but got '{value}'.");
        return numValue switch
        {
            0 => false,
            1 => true,
            _ => throw new VdfException($"Expected values [0, 1] but got {numValue}.")
        };
    }

    private static VProperty ParseFile(IFileInfo file)
    {
        try
        {
            using var textReader = file.OpenText();
            return VdfConvert.Deserialize(textReader);
        }
        catch (VdfException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new VdfException($"Failed reading {file.FullName}: {e.Message}", e);
        }
    }
}