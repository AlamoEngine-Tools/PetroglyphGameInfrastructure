using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class PetroglyphGameClients
{
    /// <summary>
    /// Adds services provided by this library to the given <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IGameExecutableFileService>(sp => new GameExecutableFileService(sp));
        serviceCollection.AddTransient<IGameExecutableNameBuilder>(_ => new GameExecutableNameBuilder());
            
        serviceCollection.AddTransient<IArgumentValidator>(_ => new ArgumentValidator());
        serviceCollection.AddTransient<IArgumentCommandLineBuilder>(sp => new ArgumentCommandLineBuilder(sp));
    }
}