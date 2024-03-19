using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal class SteamAbstractionLayer
{
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISteamWrapperFactory>(sp => new SteamWrapperFactory(sp));
        serviceCollection.AddSingleton<IProcessHelper>(sp => new ProcessHelper());
    }
}