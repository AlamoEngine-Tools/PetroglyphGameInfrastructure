using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.NativeMethods;
using AnakinRaW.CommonUtilities.Registry.Extensions;

namespace AET.SteamAbstraction;

internal class WindowsSteamWrapper(WindowsSteamRegistry registry, IServiceProvider serviceProvider)
    : SteamWrapper(registry, serviceProvider)
{
    public override bool IsRunning
    {
        get
        {
            var pid = registry.ProcessId;
            return pid is not (null or 0) && ProcessHelper.IsProcessRunning(pid.Value);
        }
    }

    private async Task WaitUntil(Func<bool> returnCondition, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (returnCondition())
            return;

        var processKey = registry.ActiveProcessKey;
        try
        {

            while (!returnCondition())
            {
                token.ThrowIfCancellationRequested();
                processKey ??= registry.ActiveProcessKey;
                if (processKey is null)
                    continue;
                await processKey.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
            }
        }
        finally
        {
            processKey?.Dispose();
        }
    }

    protected internal override async Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        await WaitUntil(() => IsUserLoggedIn, token);
    }

    protected internal override async Task WaitSteamRunningAsync(CancellationToken token)
    {
        await WaitUntil(() => IsRunning, token);
    }

    protected override void ResetCurrentUser()
    {
        registry.ActiveUserId = 0;
    }

    protected internal override uint GetCurrentUserId()
    {
        return registry.ActiveUserId ?? 0;
    }

    protected internal override async Task WaitSteamOfflineRunning(CancellationToken token)
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
            await Task.Delay(750, token).ConfigureAwait(false);
        }

        return;

        // If anybody knows a smarter way, let me know!
        static bool GetSteamMainWindowHandle()
        {
            var processes = Process.GetProcessesByName("steamwebhelper");
            var p = processes.FirstOrDefault(x => x.MainWindowHandle != IntPtr.Zero);
            try
            {
                if (p is null)
                    return false;
                
                var handle = p.MainWindowHandle;
                var c = new WindowHandleInfo(handle).GetAllChildHandles();
                var text = User32.GetWindowTitle(handle);

                // Empty string typically is the Installer/Updater dialog.
                // We don't want to early-exit on this one!
                if (string.IsNullOrEmpty(text))
                    return false;

                // The Steam-Client has children while the selection dialog (Go Online/Stay Offline) has none.
                return c.Any();
            }
            finally
            {
                foreach (var toDispose in processes) 
                    toDispose?.Dispose();
            }
        }
    }
}