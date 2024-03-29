using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Games;
using AnakinRaW.CommonUtilities;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal abstract class SteamWrapper(ISteamRegistry registry, IServiceProvider serviceProvider) : DisposableObject, ISteamWrapper
{

    protected ISteamRegistry Registry { get; } = registry ?? throw new ArgumentNullException(nameof(registry));

    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    protected IFileSystem FileSystem { get; } = serviceProvider.GetRequiredService<IFileSystem>();

    public bool Installed => Registry.ExecutableFile?.Exists ?? false;

    public abstract bool IsRunning { get; }

    public bool? UserWantsOfflineMode
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

    public bool IsUserLoggedIn
    {
        get
        {
            if (!IsRunning)
                return false;
            return Registry.ActiveUserId is not (null or 0);
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

        using var gameFinder = ServiceProvider.GetRequiredService<ISteamGameFinder>();
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
                FileName = Registry.ExecutableFile!.FullName,
                UseShellExecute = false
            }
        };
        process.Start();
    }

    public async Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning,
        CancellationToken cancellation = default)
    {
        ThrowIfSteamNotInstalled();
        if (!IsRunning)
        {
            // Required because an external kill (e.g. by taskmgr) does not reset this value, so we have to do this manually
            Registry.ActiveUserId = 0;
            if (startIfNotRunning)
            {
                StartSteam();
                await WaitSteamRunningAsync(cancellation).ConfigureAwait(false);
            }

            if (!IsRunning)
                throw new SteamException("Steam is not running anymore.");
        }

        if (IsUserLoggedIn)
            return;

        var wantsOffline = UserWantsOfflineMode;
        if (wantsOffline != null && wantsOffline.Value)
        {
            await WaitSteamOfflineRunning(cancellation).ConfigureAwait(false);
        }
        else
        {
            await WaitSteamUserLoggedInAsync(cancellation).ConfigureAwait(false);
            if (Registry.ActiveUserId == 0)
                throw new SteamException("Login was not completed.");
        }
    }

    protected abstract Task WaitSteamOfflineRunning(CancellationToken token);

    protected abstract Task WaitSteamUserLoggedInAsync(CancellationToken token);

    protected abstract Task WaitSteamRunningAsync(CancellationToken token);

    private void ThrowIfSteamNotInstalled()
    {
        if (!Installed)
            throw new SteamNotFoundException();
    }

    protected override void DisposeManagedResources()
    {
        Registry.Dispose();
        base.DisposeManagedResources();
    }
}