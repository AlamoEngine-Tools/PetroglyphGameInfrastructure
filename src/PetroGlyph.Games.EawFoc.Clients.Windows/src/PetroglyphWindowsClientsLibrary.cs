using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
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
        serviceCollection.AddSingleton<ISteamWrapper>(sp => new SteamWrapper(sp));
        serviceCollection.AddSingleton<IGameClientFactory>(sp => new WindowsGameClientFactory(sp));

        // Transient Services
        serviceCollection.AddTransient<ISteamGameFinder>(sp => new SteamGameFinder(sp));
        serviceCollection.AddTransient<ISteamAppManifestReader>(sp => new SteamVdfReader(sp));
        serviceCollection.AddTransient<ILibraryConfigReader>(sp => new SteamVdfReader(sp));
        serviceCollection.AddTransient<ISteamLibraryFinder>(sp => new SteamLibraryFinder(sp));
        serviceCollection.AddTransient<IArgumentValidator>(_ => new ArgumentValidator());
        serviceCollection.AddTransient<IArgumentCommandLineBuilder>(sp => new ArgumentCommandLineBuilder(sp));
    }
}
