using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Steam;

namespace PetroGlyph.Games.EawFoc.Clients;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class PetroglyphWindowsClientsLibrary
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
       
        // Transient Services
        serviceCollection.AddTransient<ISteamGameFinder>(sp => new SteamGameFinder(sp));
    }
}