using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;

namespace AET.SteamAbstraction;

internal interface ISteamVdfReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);

    IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile);

    LoginUsers ReadLoginUsers(IFileInfo configFile);
}