using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class PetroglyphWindowsSteamClientsLibrary
{
    /// <summary>
    /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeLibraryWithDefaultServices(IServiceCollection serviceCollection)
    {
        // Singleton Services
        serviceCollection.AddSingleton<ISteamRegistry>(sp => new SteamRegistry(sp));
        serviceCollection.AddSingleton<ISteamWrapper>(sp => new SteamWrapper(sp));

        // Transient Services
        serviceCollection.AddTransient<ISteamGameFinder>(sp => new SteamGameFinder(sp));
        serviceCollection.AddTransient<ISteamAppManifestReader>(sp => new SteamVdfReader(sp));
        serviceCollection.AddTransient<ILibraryConfigReader>(sp => new SteamVdfReader(sp));
        serviceCollection.AddTransient<ISteamLibraryFinder>(sp => new SteamLibraryFinder(sp));
    }
}
