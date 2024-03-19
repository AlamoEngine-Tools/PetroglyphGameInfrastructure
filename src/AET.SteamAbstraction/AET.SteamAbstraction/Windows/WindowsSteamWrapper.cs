using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Registry.Windows;

namespace AET.SteamAbstraction;

internal class WindowsSteamWrapper(ISteamRegistry registry, IServiceProvider serviceProvider) : SteamWrapper(registry, serviceProvider)
{
    protected override async Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException();
        token.ThrowIfCancellationRequested();
        if (IsUserLoggedIn)
            return;

        while (!IsUserLoggedIn)
        {
            token.ThrowIfCancellationRequested();
            var processKey = Registry.ActiveProcessKey;
            if (processKey is null)
                return;
            if (processKey is not WindowsRegistryKey windowsRegistryKey)
                throw new InvalidOperationException("Expected Windows registry Key");


            await windowsRegistryKey.WindowsKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
        }
    }

    protected override async Task WaitSteamRunningAsync(CancellationToken token)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException();
        token.ThrowIfCancellationRequested();
        if (IsRunning)
            return;

        while (!IsRunning)
        {
            token.ThrowIfCancellationRequested();
            var processKey = Registry.ActiveProcessKey;
            if (processKey is null)
                return;
            if (processKey is not WindowsRegistryKey windowsRegistryKey)
                throw new InvalidOperationException("Expected Windows registry Key");
            await windowsRegistryKey.WindowsKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
        }
    }
}