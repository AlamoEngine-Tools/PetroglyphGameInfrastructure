using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public class SteamLibraryFinder : ISteamLibraryFinder
{
    public SteamLibraryFinder(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
    }

    public IEnumerable<SteamLibrary> FindLibraries(IDirectoryInfo steamInstallLocation)
    {
        Requires.NotNull(steamInstallLocation, nameof(steamInstallLocation));
        throw new NotImplementedException();
    }
}