using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Registry.Windows;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;


var services = SetupApplication();

var game = FindGame();
var mods = FindMods();

Console.WriteLine("Installed Mods:");
foreach (var mod in mods)
{
    Console.Write($"{mod.Name}");
    if (mod.Type == ModType.Workshops)
        Console.Write('*');
    Console.WriteLine();
}

var raw = game.FindMod(new ModReference("1129810972", ModType.Workshops)) as IPhysicalMod;


var client = services.GetRequiredService<IGameClientFactory>().CreateClient(game);

Console.WriteLine($"Playing {raw?.ToString() ?? game.ToString()}");


using var gameArgs = new GameArgumentsBuilder()
    .Add(new WindowedArgument());

if (raw is not null)
    gameArgs.AddSingleMod(raw);


client.Play(gameArgs.Build());
return;


IList<IMod> FindMods()
{
    var modList = new List<IMod>();

    var modFinder = services.GetRequiredService<IModFinder>();
    var modRefs = modFinder.FindMods(game);
    var factory = services.GetRequiredService<IModFactory>();

    foreach (var modReference in modRefs)
    {
        var mod = factory.CreatePhysicalMod(game, modReference, CultureInfo.CurrentCulture);
        modList.Add(mod);
        game.AddMod(mod);
    }

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
    SteamPetroglyphStarWarsGameClients.InitializeServices(sc);

    sc.AddSingleton<IModNameResolver>(sp => new OnlineModNameResolver(sp));
    sc.AddSingleton<IModGameTypeResolver>(sp => new OnlineModGameTypeResolver(sp));
    
    // The game detector to use for this application. 
    sc.AddSingleton<IGameDetector>(sp => new SteamPetroglyphStarWarsGameDetector(sp));

    return sc.BuildServiceProvider();
}