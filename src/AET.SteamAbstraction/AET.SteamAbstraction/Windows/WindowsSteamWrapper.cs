using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.NativeMethods;
using AET.SteamAbstraction.Utilities;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal class WindowsSteamWrapper : SteamWrapper
{
    private readonly IWindowsSteamRegistry _windowsRegistry;
    private readonly IProcessHelper _processHelper;

    public override bool IsRunning
    {
        get
        {
            var pid = _windowsRegistry.ProcessId;
            if (pid is null or 0)
                return false;
            return _processHelper.GetProcessByPid(pid.Value) != null;
        }
    }

    public WindowsSteamWrapper(IWindowsSteamRegistry registry, IServiceProvider serviceProvider) : base(registry, serviceProvider)
    {
        _windowsRegistry = registry;
        _processHelper = serviceProvider.GetRequiredService<IProcessHelper>();
    }

    public override bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? game)
    {
        ThrowIfSteamNotInstalled();
        
        game = null;
        var apps = _windowsRegistry.InstalledApps;
        if (apps is null || !apps.Contains(gameId))
            return false;
        
        return base.IsGameInstalled(gameId, out game);
    }

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
            var processKey = _windowsRegistry.ActiveProcessKey;
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
            var processKey = _windowsRegistry.ActiveProcessKey;
            if (processKey is null)
                return;
            if (processKey is not WindowsRegistryKey windowsRegistryKey)
                throw new InvalidOperationException("Expected Windows registry Key");
            await windowsRegistryKey.WindowsKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
        }
    }

    protected override async Task WaitSteamOfflineRunning(CancellationToken token)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var linkedToken = linkedCts.Token;
        try
        {
            var mainWindowTask = Task.Run(() => WaitMainWindowIsSteamClient(linkedToken), token);
            var userLoginTask = Task.Run(() => WaitSteamUserLoggedInAsync(linkedToken), token);
            await Task.WhenAny(mainWindowTask, userLoginTask).ConfigureAwait(false);
        }
        finally
        {
            linkedCts.Cancel();
            linkedCts.Dispose();
        }
    }

    private async Task WaitMainWindowIsSteamClient(CancellationToken token)
    {
        while (IsRunning)
        {
            token.ThrowIfCancellationRequested();
            var mainWindowHasChildren = GetSteamMainWindowHandle();
            if (mainWindowHasChildren)
                return;
            // Just some arbitrary waiting 
            await Task.Delay(750, token);
        }


        // If anybody knows a smarter way, let me know!
        static bool GetSteamMainWindowHandle()
        {
            var p = Process.GetProcessesByName("steam").FirstOrDefault();
            try
            {
                var handle = p?.MainWindowHandle;
                if (handle is null)
                    return false;
                var c = new WindowHandleInfo(handle.Value).GetAllChildHandles();
                var text = User32.GetWindowTitle(handle.Value);

                // Empty string typically is the Installer/Updater dialog.
                // We don't want to early-exit on this one!
                if (string.IsNullOrEmpty(text))
                    return false;

                // The Steam-Client has children while the selection dialog (Go Online/Stay Offline) has none.
                return c.Any();
            }
            finally
            {
                p?.Dispose();
            }
        }
    }
}