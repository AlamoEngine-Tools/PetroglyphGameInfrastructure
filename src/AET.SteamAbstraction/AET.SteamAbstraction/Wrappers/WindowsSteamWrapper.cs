using System;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Threading;

namespace AET.SteamAbstraction;

internal class WindowsSteamWrapper(IServiceProvider serviceProvider) : SteamWrapper(serviceProvider)
{
    protected override Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        throw new NotImplementedException();

        //token.ThrowIfCancellationRequested();
        //if (IsUserLoggedIn)
        //    return;

        //while (!IsUserLoggedIn)
        //{
        //    token.ThrowIfCancellationRequested();
        //    var processKey = Registry.ActiveProcessKey;
        //    if (processKey is null)
        //        return;
        //    if (processKey is not WindowsRegistryKey windowsRegistryKey)
        //        throw new InvalidOperationException("Expected Windows registry Key");


        //    await windowsRegistryKey.WindowsKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
        //}
    }

    protected override Task WaitSteamRunningAsync(CancellationToken token)
    {
        throw new NotImplementedException();

        //token.ThrowIfCancellationRequested();
        //if (IsRunning)
        //    return;

        //while (!IsRunning)
        //{
        //    token.ThrowIfCancellationRequested();
        //    var processKey = Registry.ActiveProcessKey;
        //    if (processKey is null)
        //        return;
        //    if (processKey is not WindowsRegistryKey windowsRegistryKey)
        //        throw new InvalidOperationException("Expected Windows registry Key");
        //    await windowsRegistryKey.WindowsKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
        //}
    }
}