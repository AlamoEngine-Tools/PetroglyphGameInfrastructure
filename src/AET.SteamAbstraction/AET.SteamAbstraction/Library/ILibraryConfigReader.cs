using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Library;

internal interface ILibraryConfigReader
{
    IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile);
}