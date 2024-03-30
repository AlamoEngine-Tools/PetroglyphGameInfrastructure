using System;
using System.Linq;
using AET.SteamAbstraction.Library;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction.Games;

internal class SteamGameFinder : DisposableObject, ISteamGameFinder
{
    private readonly ISteamLibraryFinder _libraryFinder;

    public SteamGameFinder(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));

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

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        _libraryFinder.Dispose();
    }
}