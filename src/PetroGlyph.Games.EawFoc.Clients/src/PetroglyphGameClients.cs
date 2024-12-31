using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class PetroglyphGameClients
{
    /// <summary>
    /// Adds services provided by this library to the specified <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameClientFactory>(sp => new GameClientFactory(sp));
        serviceCollection.AddSingleton<IGameProcessLauncher>(sp => new GameProcessLauncher(sp));
    }
}