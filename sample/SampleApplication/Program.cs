using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Clients;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;


var sp = SetupApplication();

var game = FindGame();
var mods = FindMods();

var client = sp.GetRequiredService<IGameClientFactory>().CreateClient(game.Platform, sp);

client.Play((IPlayableObject)mods.FirstOrDefault() ?? game);


IEnumerable<IMod> FindMods()
{
    var mods = new List<IMod>();

    var modFinder = sp.GetRequiredService<IModReferenceFinder>();
    var modRefs = modFinder.FindMods(game);
    var factory = sp.GetRequiredService<IModFactory>();

    foreach (var modReference in modRefs)
    {
        var mod = factory.FromReference(game, modReference);
        mods.AddRange(mod);
    }

    foreach (var mod in mods) 
        game.AddMod(mod);

    // Mods need to be added to the game first, before resolving their dependencies.
    foreach(var mod in mods)
    {
        var resolver = sp.GetRequiredService<IDependencyResolver>();
        mod.ResolveDependencies(resolver,
            new DependencyResolverOptions { CheckForCycle = true, ResolveCompleteChain = true });
    }

    return mods;

}

IGame FindGame()
{
    var detector = sp.GetRequiredService<IGameDetector>();
    var detectionResult = detector.Detect(new GameDetectorOptions(GameType.Foc));
    var gameFactory = sp.GetRequiredService<IGameFactory>();
    return gameFactory.CreateGame(detectionResult);
}


IServiceProvider SetupApplication()
{
    var sc = new ServiceCollection();
    PetroglyphGameInfrastructureLibrary.InitializeLibraryWithDefaultServices(sc);
    PetroglyphClientsLibrary.InitializeLibraryWithDefaultServices(sc);
    PetroglyphWindowsSteamClientsLibrary.InitializeLibraryWithDefaultServices(sc);

    sc.AddSingleton(WindowsRegistry.Default);
    sc.AddTransient<IGameDetector>(sp => new SteamPetroglyphStarWarsGameDetector(sp));
    sc.AddTransient<IGameFactory>(sp => new GameFactory(sp));
    sc.AddTransient<IModReferenceFinder>(sp => new FileSystemModFinder(sp));
    sc.AddTransient<IModFactory>(sp => new ModFactory(sp));
    sc.AddTransient<IModReferenceLocationResolver>(sp => new ModReferenceLocationResolver(sp));
    sc.AddTransient<IModNameResolver>(sp => new DirectoryModNameResolver(sp));
    sc.AddTransient<IDependencyResolver>(sp => new ModDependencyResolver(sp));
    sc.AddTransient<IGameClientFactory>(sp => new DefaultGameClientFactory(sp));

    return sc.BuildServiceProvider();
}