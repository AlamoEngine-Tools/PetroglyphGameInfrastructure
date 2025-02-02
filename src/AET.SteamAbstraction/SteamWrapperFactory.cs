using System;
using System.Diagnostics;
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
            Debug.Assert(registry is WindowsSteamRegistry);
            return new WindowsSteamWrapper((WindowsSteamRegistry)registry, serviceProvider);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new NotImplementedException("Steam wrapper for Linux is not yet implemented.");
        }

        throw new PlatformNotSupportedException("The current platform is not supported.");
    }
}