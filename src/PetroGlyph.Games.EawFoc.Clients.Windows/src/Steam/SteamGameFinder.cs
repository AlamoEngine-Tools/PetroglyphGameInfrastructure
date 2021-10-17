using System;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public class SteamGameFinder : ISteamGameFinder
{
    private readonly ISteamLibraryFinder _libraryFinder;

    public SteamGameFinder(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _libraryFinder = serviceProvider.GetService<ISteamLibraryFinder>() ?? new SteamLibraryFinder(serviceProvider);
    }

    public SteamAppManifest? FindGame(IDirectoryInfo steamInstallLocation, uint gameId)
    {
        Requires.NotNull(steamInstallLocation, nameof(steamInstallLocation));
        var libraries = _libraryFinder.FindLibraries(steamInstallLocation);

        var game = libraries
            .Select(lib => lib.Apps.FirstOrDefault(a => a.Id == gameId))
            .FirstOrDefault(matching => matching is not null);
        return game;
    }
}