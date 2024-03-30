using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

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
        serviceCollection.AddTransient<IGameProcessLauncher>(sp => new DefaultGameProcessLauncher(sp));
        serviceCollection.AddTransient<IGameExecutableFileService>(sp => new GameExecutableFileService(sp));
        serviceCollection.AddTransient<IGameExecutableNameBuilder>(_ => new GameExecutableNameBuilder());

        serviceCollection.AddTransient<IGameClientFactory>(sp => new DefaultGameClientFactory(sp));
        serviceCollection.AddTransient<IModArgumentListFactory>(sp => new ModArgumentListFactory(sp));
        serviceCollection.AddTransient<IArgumentCollectionBuilder>(sp => new KeyBasedArgumentCollectionBuilder());
        serviceCollection.AddTransient<IArgumentValidator>(_ => new ArgumentValidator());
        serviceCollection.AddTransient<IArgumentCommandLineBuilder>(sp => new ArgumentCommandLineBuilder(sp));
    }
}