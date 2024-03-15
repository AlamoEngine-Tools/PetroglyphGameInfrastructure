using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.SteamAbstraction;

internal interface ILibraryConfigReader
{
    IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile);
}