using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.NativeMethods;
using AET.SteamAbstraction.Utilities;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal abstract class SteamWrapper(IServiceProvider serviceProvider) : ISteamWrapper
{

    private readonly IProcessHelper _processHelper = serviceProvider.GetRequiredService<IProcessHelper>();

    protected ISteamRegistry Registry { get; } = serviceProvider.GetRequiredService<ISteamRegistry>();
    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    protected IFileSystem FileSystem { get; } = serviceProvider.GetRequiredService<IFileSystem>();

    public bool Installed => Registry.ExeFile?.Exists ?? false;

    public bool IsRunning
    {
        get
        {
            var pid = Registry.ProcessId;
            if (pid is null or 0)
                return false;
            return _processHelper.GetProcessByPid(pid.Value) != null;
        }
    }

    public bool? WantOfflineMode
    {
        get
        {
            var steamDirectory = Registry.InstallationDirectory;
            if (steamDirectory is null || !steamDirectory.Exists)
                return null;

            var configFile = FileSystem.Path.Combine(steamDirectory.FullName, "config/loginusers.vdf");
            if (!FileSystem.File.Exists(configFile))
                return null;

            try
            {
                var config = VdfConvert.Deserialize(FileSystem.File.ReadAllText(configFile)).ToJson();
                var usersWantingOffline = config.SelectTokens("$..[?(@.WantsOfflineMode=='1')]").ToList();
                if (!usersWantingOffline.Any())
                    return false;

                var anyMostRecent = config.SelectTokens("$..[?(@.mostrecent)]");
                if (!anyMostRecent.Any())
                    return true;

                var mostRecent = usersWantingOffline.FirstOrDefault(x => x.SelectToken("$..[?(@.mostrecent=='1')]") != null);
                return mostRecent is not null;
            }
            catch
            {
                return null;
            }
        }
    }

    internal bool IsUserLoggedIn
    {
        get
        {
            var userId = Registry.ActiveUserId;
            return userId is not (null or 0);
        }
    }

    public bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? game)
    {
        ThrowIfSteamNotInstalled();
        game = null;
        var apps = Registry.InstalledApps;
        if (apps is null)
            return false;

        if (!apps.Contains(gameId))
            return false;

        var gameFinder = ServiceProvider.GetRequiredService<ISteamGameFinder>();
        game = gameFinder.FindGame(gameId);
        return game is not null;
    }

    public void StartSteam()
    {
        ThrowIfSteamNotInstalled();
        if (IsRunning)
            return;
        var process = new Process
        {
            StartInfo =
            {
                FileName = Registry.ExeFile!.FullName,
                UseShellExecute = false
            }
        };
        process.Start();
    }

    public async Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning,
        CancellationToken cancellation = default)
    {
        ThrowIfSteamNotInstalled();
        var running = IsRunning;
        if (!running)
        {
            // Required because a taskmgr kill does not reset this value, so we have to do this manually
            Registry.ActiveUserId = 0;
            if (startIfNotRunning)
                StartSteam();
            await WaitSteamRunningAsync(cancellation);
        }

        if (IsUserLoggedIn)
            return;

        var wantsOffline = WantOfflineMode;
        if (wantsOffline != null && wantsOffline.Value)
        {
            await Task.Delay(2000, cancellation);
            await WaitSteamOfflineRunning(cancellation);
            cancellation.ThrowIfCancellationRequested();
            if (!IsRunning)
                throw new SteamException("Steam is not running anymore.");
        }
        else
        {
            await WaitSteamUserLoggedInAsync(cancellation);
        }
    }

    private async Task WaitSteamOfflineRunning(CancellationToken token)
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

    protected abstract Task WaitSteamUserLoggedInAsync(CancellationToken token);

    protected abstract Task WaitSteamRunningAsync(CancellationToken token);

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

    private void ThrowIfSteamNotInstalled()
    {
        if (!Installed)
            throw new SteamException("Steam is not installed!");
    }
}