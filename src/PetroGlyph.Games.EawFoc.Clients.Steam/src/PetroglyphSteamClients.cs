using Microsoft.Extensions.DependencyInjection;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class PetroglyphSteamClientsLibrary
{
    /// <summary>
    /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeLibraryWithDefaultServices(IServiceCollection serviceCollection)
    {
        // Transient Services
        serviceCollection.AddTransient<IGameExecutableNameBuilder>(sp => new SteamExecutableNameBuilder(sp));
    }
}