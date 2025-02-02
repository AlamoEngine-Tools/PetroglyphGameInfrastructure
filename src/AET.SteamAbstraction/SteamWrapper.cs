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
using AET.SteamAbstraction.Utilities;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal abstract class SteamWrapper(ISteamRegistry registry, IServiceProvider serviceProvider) : DisposableObject, ISteamWrapper
{
    protected readonly ISteamRegistry Registry = registry ?? throw new ArgumentNullException(nameof(registry));
    protected readonly IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    protected readonly ISteamLibraryFinder LibraryFinder = serviceProvider.GetRequiredService<ISteamLibraryFinder>();
    protected readonly IProcessHelper ProcessHelper = serviceProvider.GetRequiredService<IProcessHelper>();

    // We are checking both, Exe path and install dir, because they might be on different locations
    public bool Installed
    {
        get
        {
            ThrowIfDisposed();
            return Registry.ExecutableFile?.Exists == true && Registry.InstallationDirectory?.Exists == true;
        }
    }

    public abstract bool IsRunning { get; }

    public bool? UserWantsOfflineMode
    {
        get
        {
            ThrowIfDisposed();
            var steamDirectory = Registry.InstallationDirectory;
            if (steamDirectory is null || !steamDirectory.Exists)
                return null;

            var configFile = FileSystem.Path.Combine(steamDirectory.FullName, "config", "loginusers.vdf");
            if (!FileSystem.File.Exists(configFile))
                return null;

            try
            {
                var config = SteamVdfReader.ReadLoginUsers(FileSystem.FileInfo.New(configFile));
                return config.Users.Any(user => user.MostRecent && user.UserWantsOffline);
            }
            catch (SteamException)
            {
                return null;
            }
        }
    }

    public IEnumerable<ISteamLibrary> Libraries
    {
        get
        {
            ThrowIfDisposed();
            if (!Installed)
                return [];
            return LibraryFinder.FindLibraries(Registry.InstallationDirectory!);
        }
    }

    public bool IsUserLoggedIn
    {
        get
        {
            ThrowIfDisposed();
            if (!IsRunning)
                return false;
            return GetCurrentUserId() is not 0;
        }
    }

    public bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? game)
    {
        ThrowIfDisposed();
        ThrowIfSteamNotInstalled();
        game = Libraries.SelectMany(library => library.GetApps()).FirstOrDefault(manifest => manifest.Id == gameId);
        return game is not null;
    }

    public void StartSteam()
    {
        ThrowIfDisposed();
        ThrowIfSteamNotInstalled();
        if (IsRunning)
            return;
        ProcessHelper.StartProcess(new ProcessStartInfo
        {
            FileName = Registry.ExecutableFile!.FullName,
            UseShellExecute = false
        });
    }

    public async Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning,
        CancellationToken cancellation = default)
    {
        ThrowIfDisposed();
        ThrowIfSteamNotInstalled();
        if (!IsRunning)
        {
            // This is required, because if Steam was closed or killed,
            // there might be no chance to tell when exactly a user is logged in. 
            // In other words, if we don't reset the value, it might be possible,
            // that this method returns, when in fact the login has not yet completed.
            ResetCurrentUser();
            if (startIfNotRunning)
            {
                StartSteam();
                await WaitSteamRunningAsync(cancellation).ConfigureAwait(false);
            }

            if (!IsRunning)
                throw new SteamException("Steam is not running.");
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
        }
    }

    protected abstract void ResetCurrentUser();

    protected internal abstract uint GetCurrentUserId();

    protected internal abstract Task WaitSteamOfflineRunning(CancellationToken token);

    protected internal abstract Task WaitSteamUserLoggedInAsync(CancellationToken token);

    protected internal abstract Task WaitSteamRunningAsync(CancellationToken token);

    protected void ThrowIfSteamNotInstalled()
    {
        if (!Installed)
            throw new SteamNotFoundException();
    }

    protected override void DisposeResources()
    {
        Registry.Dispose();
        base.DisposeResources();
    }
}