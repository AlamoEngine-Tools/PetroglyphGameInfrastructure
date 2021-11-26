using System.Collections.Generic;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal interface ILibraryConfigReader
{
    IEnumerable<IDirectoryInfo> ReadLibraryLocationsFromConfig(IFileInfo configFile);
}