using System.Collections.Generic;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public interface ISteamLibraryFinder
{
    IEnumerable<SteamLibrary> FindLibraries(IDirectoryInfo steamInstallLocation);
}