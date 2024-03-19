using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal class SteamAbstractionLayer
{
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISteamWrapperFactory>(sp => new SteamWrapperFactory(sp));
        serviceCollection.AddSingleton<ISteamRegistryFactory>(sp => new SteamRegistryFactory(sp));
        serviceCollection.AddSingleton<IProcessHelper>(sp => new ProcessHelper());

        serviceCollection.AddSingleton<ILibraryConfigReader>(sp => new SteamVdfReader(sp));
        serviceCollection.AddSingleton<ISteamAppManifestReader>(sp => new SteamVdfReader(sp));

        serviceCollection.AddTransient<ISteamLibraryFinder>(sp => new SteamLibraryFinder(sp));
        serviceCollection.AddTransient<ISteamGameFinder>(sp => new SteamGameFinder(sp));
    }
}