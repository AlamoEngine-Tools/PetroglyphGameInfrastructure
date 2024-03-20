using System;
using System.Collections.Generic;

namespace AET.SteamAbstraction.Library;

internal interface ISteamLibraryFinder : IDisposable
{
    IEnumerable<ISteamLibrary> FindLibraries();
}