﻿using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal class SteamGameFinder : ISteamGameFinder
{
    private readonly ISteamLibraryFinder _libraryFinder;

    public SteamGameFinder(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _libraryFinder = serviceProvider.GetRequiredService<ISteamLibraryFinder>();
    }

    public SteamAppManifest? FindGame(uint gameId)
    {
        var libraries = _libraryFinder.FindLibraries();
        var game = libraries
            .Select(lib => lib.GetApps().FirstOrDefault(a => a.Id == gameId))
            .FirstOrDefault(matching => matching is not null);
        return game;
    }
}