using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
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
        serviceCollection.AddSingleton<ISteamGameHelpers>(sp => new SteamGameHelpers(sp));
        serviceCollection.AddSingleton<IGameFactory>(sp => new GameFactory(sp));
        serviceCollection.AddSingleton<IModFactory>(sp => new ModFactory(sp));
        serviceCollection.AddSingleton<IModIdentifierBuilder>(sp => new ModIdentifierBuilder(sp));
        serviceCollection.AddSingleton<IModReferenceFinder>(sp => new FileSystemModFinder(sp));
        serviceCollection.AddSingleton<IModReferenceLocationResolver>(sp => new ModReferenceLocationResolver(sp));
        serviceCollection.AddSingleton<IModDependencyGraphBuilder>(_ => new ModDependencyGraphBuilder());
        serviceCollection.AddSingleton<IModDependencyTraverser>(sp => new ModDependencyTraverser(sp));

        // Must be transient
        serviceCollection.AddTransient<IDependencyResolver>(sp => new ModDependencyResolver(sp));
    }
}