using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal abstract class SteamWrapper(ISteamRegistry registry, IServiceProvider serviceProvider) : DisposableObject, ISteamWrapper
{
    protected readonly ISteamRegistry Registry = registry ?? throw new ArgumentNullException(nameof(registry));
    protected readonly IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    protected readonly ISteamLibraryFinder LibraryFinder = serviceProvider.GetRequiredService<ISteamLibraryFinder>();

    // We are checking both, Exe path and install dir, because they might be on different locations
    public bool Installed => Registry.ExecutableFile?.Exists == true && Registry.InstallationDirectory?.Exists == true;

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
                var config = SteamVdfReader.ReadLoginUsers(FileSystem.FileInfo.New(configFile));
                return config.Users.Any(user => user.MostRecent && user.UserWantsOffline);
            }
            catch
            {
                return null;
            }
        }
    }

    public IEnumerable<ISteamLibrary> Libraries => LibraryFinder.FindLibraries();

    public bool IsUserLoggedIn
    {
        get
        {
            if (!IsRunning)
                return false;
            return GetCurrentUserId() is not (null or 0);
        }
    }

    public virtual bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? game)
    {
        ThrowIfSteamNotInstalled();
        game = Libraries.SelectMany(library => library.GetApps()).FirstOrDefault(manifest => manifest.Id == gameId);
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
            ResetCurrentUser();
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
            if (GetCurrentUserId() == 0)
                throw new SteamException("Login was not completed.");
        }
    }

    protected abstract void ResetCurrentUser();

    protected abstract int? GetCurrentUserId();

    protected abstract Task WaitSteamOfflineRunning(CancellationToken token);

    protected abstract Task WaitSteamUserLoggedInAsync(CancellationToken token);

    protected abstract Task WaitSteamRunningAsync(CancellationToken token);

    protected void ThrowIfSteamNotInstalled()
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