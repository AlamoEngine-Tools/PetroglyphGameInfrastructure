using System.Collections.Generic;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ILibraryConfigReader
{
    IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile);
}