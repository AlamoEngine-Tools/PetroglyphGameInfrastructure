using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public static class PetroglyphGameInfrastructureLibrary
{
    /// <summary>
    /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeLibraryWithDefaultServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IGameRegistryFactory>(_ => new GameRegistryFactory());
        serviceCollection.AddTransient<IModIdentifierBuilder>(sp => new ModIdentifierBuilder(sp));
        serviceCollection.AddTransient<ISteamGameHelpers>(sp => new SteamGameHelpers(sp));
    }
}