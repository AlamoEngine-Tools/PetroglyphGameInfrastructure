using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class SteamPetroglyphStarWarsGameClients
{
    /// <summary>
    /// Adds services provided by this library to the specified <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameClientFactory>(sp => new SteamGameClientsFactory(sp));
    }
}