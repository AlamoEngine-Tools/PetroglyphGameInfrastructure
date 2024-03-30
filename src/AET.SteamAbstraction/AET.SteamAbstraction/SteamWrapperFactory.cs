using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal class SteamWrapperFactory(IServiceProvider serviceProvider) : ISteamWrapperFactory
{
    private readonly ISteamRegistryFactory _registryFactory = serviceProvider.GetRequiredService<ISteamRegistryFactory>();

    public ISteamWrapper CreateWrapper()
    {
        var registry = _registryFactory.CreateRegistry();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (registry is not WindowsSteamRegistry windowsRegistry)
                throw new InvalidOperationException("Expected Windows Registry on Windows systems.");
            return new WindowsSteamWrapper(windowsRegistry, serviceProvider);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxSteamWrapper(registry, serviceProvider);

        throw new PlatformNotSupportedException($"The current platform is not supported.");
    }
}