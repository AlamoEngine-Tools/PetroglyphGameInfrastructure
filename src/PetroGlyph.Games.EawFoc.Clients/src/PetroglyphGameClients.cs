using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Services.Language;

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
        serviceCollection.AddSingleton<IGameProcessLauncher>(sp => new DefaultGameProcessLauncher(sp));
        serviceCollection.AddSingleton<IGameExecutableFileService>(sp => new GameExecutableFileService(sp));
        serviceCollection.AddSingleton<IGameExecutableNameBuilder>(_ => new GameExecutableNameBuilder());

        serviceCollection.AddSingleton<IGameClientFactory>(sp => new GameClientFactory(sp));
        serviceCollection.AddSingleton<IModArgumentListFactory>(sp => new ModArgumentListFactory(sp));
        serviceCollection.AddSingleton<IArgumentValidator>(_ => new ArgumentValidator());
        serviceCollection.AddSingleton<IArgumentCommandLineBuilder>(sp => new ArgumentCommandLineBuilder(sp));

        serviceCollection.AddSingleton<ILanguageFinder>(sp => new SteamGameLanguageFinder(sp));
    }
}