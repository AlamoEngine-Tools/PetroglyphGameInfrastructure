using System;
using System.Runtime.InteropServices;

namespace AET.SteamAbstraction;

internal class SteamWrapperFactory(IServiceProvider serviceProvider) : ISteamWrapperFactory
{
    public ISteamWrapper CreateWrapper()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsSteamWrapper(new WindowsSteamRegistry(serviceProvider), serviceProvider);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxSteamWrapper(new LinuxSteamRegistry(serviceProvider), serviceProvider);

        throw new PlatformNotSupportedException($"The current platform is not supported.");
    }
}