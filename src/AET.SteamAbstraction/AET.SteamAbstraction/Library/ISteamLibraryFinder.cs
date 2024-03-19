using System.Collections.Generic;

namespace AET.SteamAbstraction.Library;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries();
}