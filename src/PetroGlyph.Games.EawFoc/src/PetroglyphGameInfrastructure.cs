using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Services.Name;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public static class PetroglyphGameInfrastructure
{
    /// <summary>
    /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameRegistryFactory>(sp => new GameRegistryFactory(sp));
        serviceCollection.AddSingleton<IGamePlatformIdentifier>(sp => new GamePlatformIdentifier(sp));
        serviceCollection.AddSingleton<ISteamGameHelpers>(sp => new SteamGameHelpers(sp));
        serviceCollection.AddSingleton<IGameFactory>(sp => new GameFactory(sp));
        //serviceCollection.AddSingleton<IModFactory>(sp => new ModFactory(sp));
        serviceCollection.AddSingleton<IModFinder>(sp => new ModFinder(sp));
        serviceCollection.AddSingleton<IModReferenceLocationResolver>(sp => new ModReferenceLocationResolver(sp));

        serviceCollection.AddSingleton(sp => new ModDependencyResolver(sp));
        serviceCollection.AddSingleton(_ => new ModDependencyGraphBuilder());
        serviceCollection.AddSingleton<IModDependencyTraverser>(sp => new ModDependencyTraverser(sp));

        serviceCollection.AddSingleton<ILanguageFinder>(sp => new InstalledLanguageFinder(sp));
        serviceCollection.AddSingleton<IIconFinder>(_ => new IconFinder());

        serviceCollection.AddSingleton<ISteamWorkshopCache>(_ => new KnownSteamWorkshopCache());
        serviceCollection.AddSingleton<ISteamWorkshopWebpageDownloader>(_ => new SteamWorkshopWebpageDownloader());

        serviceCollection.AddSingleton<IModNameResolver>(sp => new DirectoryModNameResolver(sp));
        serviceCollection.AddSingleton<IGameNameResolver>(_ => new EnglishGameNameResolver());

        serviceCollection.AddSingleton<IModGameTypeResolver>(sp => new OfflineModGameTypeResolver(sp));
    }
}