using System;
using System.Runtime.InteropServices;

namespace AET.SteamAbstraction;

internal class SteamWrapperFactory : ISteamWrapperFactory
{
    public ISteamWrapper CreateWrapper(IServiceProvider serviceProvider)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsSteamWrapper(serviceProvider);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxSteamWrapper(serviceProvider);
        throw new PlatformNotSupportedException($"The current platform is not supported.");
    }
}