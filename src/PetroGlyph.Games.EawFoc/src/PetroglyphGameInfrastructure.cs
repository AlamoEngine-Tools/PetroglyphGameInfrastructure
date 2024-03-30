using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
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
        serviceCollection.AddTransient<IGameRegistryFactory>(_ => new GameRegistryFactory());
        serviceCollection.AddTransient<IModIdentifierBuilder>(sp => new ModIdentifierBuilder(sp));
        serviceCollection.AddTransient<ISteamGameHelpers>(sp => new SteamGameHelpers(sp));

        serviceCollection.AddTransient<IGameFactory>(sp => new GameFactory(sp));
        serviceCollection.AddTransient<IGameRegistryFactory>(sp => new GameRegistryFactory());
        serviceCollection.AddTransient<IModReferenceFinder>(sp => new FileSystemModFinder(sp));
        serviceCollection.AddTransient<IModFactory>(sp => new ModFactory(sp));
        serviceCollection.AddTransient<IModReferenceLocationResolver>(sp => new ModReferenceLocationResolver(sp));
        serviceCollection.AddTransient<IModNameResolver>(sp => new DirectoryModNameResolver(sp));

        serviceCollection.AddTransient<IDependencyResolver>(sp => new ModDependencyResolver(sp));
        serviceCollection.AddTransient<IModDependencyGraphBuilder>(sp => new ModDependencyGraphBuilder());
        serviceCollection.AddTransient<IModDependencyTraverser>(sp => new ModDependencyTraverser(sp));
    }
}