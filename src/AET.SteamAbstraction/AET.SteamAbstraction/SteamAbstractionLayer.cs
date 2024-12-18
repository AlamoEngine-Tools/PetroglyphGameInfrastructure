using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public static class SteamAbstractionLayer
{
    /// <summary>
    /// Adds services provided by this library to the specified <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISteamWrapperFactory>(sp => new SteamWrapperFactory(sp));
        serviceCollection.AddSingleton<ISteamRegistryFactory>(sp => new SteamRegistryFactory(sp));
        serviceCollection.AddSingleton<IProcessHelper>(sp => new ProcessHelper());

        serviceCollection.AddSingleton<ISteamVdfReader>(sp => new SteamVdfReader(sp));

        serviceCollection.AddSingleton<ISteamLibraryFinder>(sp => new SteamLibraryFinder(sp));
        serviceCollection.AddSingleton<ISteamGameFinder>(sp => new SteamGameFinder(sp));
    }
}