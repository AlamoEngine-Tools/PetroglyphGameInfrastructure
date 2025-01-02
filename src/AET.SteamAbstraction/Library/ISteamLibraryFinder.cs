using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Library;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries(IDirectoryInfo steamInstallDir);
}