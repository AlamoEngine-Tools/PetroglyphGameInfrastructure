using System;
using System.Runtime.InteropServices;

namespace AET.SteamAbstraction.Registry;

internal class SteamRegistryFactory(IServiceProvider serviceProvider) : ISteamRegistryFactory
{
    public ISteamRegistry CreateRegistry()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsSteamRegistry(serviceProvider);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxSteamRegistry(serviceProvider);

        throw new PlatformNotSupportedException("The current platform is not supported.");
    }
}