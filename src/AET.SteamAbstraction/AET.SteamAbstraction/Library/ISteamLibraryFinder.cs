using System.Collections.Generic;

namespace AET.SteamAbstraction;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries();
}