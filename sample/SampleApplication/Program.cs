using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;


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

    sc.AddSingleton(WindowsRegistry.Default);
    sc.AddSingleton<IFileSystem>(_ => new FileSystem());

    PetroglyphGameInfrastructure.InitializeServices(sc);
    SteamAbstractionLayer.InitializeServices(sc);
    PetroglyphGameClients.InitializeServices(sc);
    
    // The game detector to use for this application. 
    sc.AddTransient<IGameDetector>(sp => new SteamPetroglyphStarWarsGameDetector(sp));

    return sc.BuildServiceProvider();
}