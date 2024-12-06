using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;


var services = SetupApplication();

var game = FindGame();
var mods = FindMods();

var client = services.GetRequiredService<IGameClientFactory>().CreateClient(game.Platform, services);


var firstMod = mods.FirstOrDefault();

Console.WriteLine($"Playing {firstMod}");

client.Play((IPlayableObject)firstMod ?? game, new ArgumentCollection(new List<IGameArgument>
{
    new WindowedArgument()
}));
return;


IList<IMod> FindMods()
{
    var modList = new List<IMod>();

    var modFinder = services.GetRequiredService<IModReferenceFinder>();
    var modRefs = modFinder.FindMods(game);
    var factory = services.GetRequiredService<IModFactory>();

    foreach (var modReference in modRefs)
    {
        var mod = factory.FromReference(game, modReference, CultureInfo.CurrentCulture);
        modList.Add(mod);
    }

    foreach (var mod in modList) 
        game.AddMod(mod);

    // Mods need to be added to the game first, before resolving their dependencies.
    foreach(var mod in modList) 
        mod.ResolveDependencies();

    return modList;

}

IGame FindGame()
{
    var detector = services.GetRequiredService<IGameDetector>();
    var detectionResult = detector.Detect(GameType.Foc);
    var gameFactory = services.GetRequiredService<IGameFactory>();
    return gameFactory.CreateGame(detectionResult, CultureInfo.CurrentCulture);
}


IServiceProvider SetupApplication()
{
    var sc = new ServiceCollection();

    sc.AddSingleton(WindowsRegistry.Default);
    sc.AddSingleton<IFileSystem>(_ => new FileSystem());

    PetroglyphGameInfrastructure.InitializeServices(sc);
    SteamAbstractionLayer.InitializeServices(sc);
    PetroglyphGameClients.InitializeServices(sc);

    sc.AddSingleton<IModNameResolver>(sp => new CompositeModNameResolver(sp, s => new List<IModNameResolver>
    {
        new OfflineWorkshopNameResolver(sp),
        new OnlineWorkshopNameResolver(sp),
        new DirectoryModNameResolver(sp)
    }));

    sc.AddSingleton<IModGameTypeResolver>(sp => new OnlineModGameTypeResolver(sp));
    
    // The game detector to use for this application. 
    sc.AddSingleton<IGameDetector>(sp => new SteamPetroglyphStarWarsGameDetector(sp));

    return sc.BuildServiceProvider();
}