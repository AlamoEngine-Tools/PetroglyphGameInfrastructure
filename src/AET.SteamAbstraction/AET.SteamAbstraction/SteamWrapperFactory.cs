using System;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Registry;
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
            if (registry is not IWindowsSteamRegistry windowsRegistry)
                throw new InvalidOperationException("Expected Windows Registry on Windows systems.");
            return new WindowsSteamWrapper(windowsRegistry, serviceProvider);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new NotImplementedException("Steam wrapper for Linux is not yet implemented.");
        }

        throw new PlatformNotSupportedException($"The current platform is not supported.");
    }
}