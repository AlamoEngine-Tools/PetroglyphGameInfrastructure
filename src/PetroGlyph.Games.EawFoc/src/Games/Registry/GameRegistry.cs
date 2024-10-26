using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Games.Registry;

internal sealed class GameRegistry : IGameRegistry
{
    internal const string VersionKey = "1.0";
    internal const string CDKeyProperty = "CD Key";
    internal const string EawGoldProperty = "EAWGold";
    internal const string ExePathProperty = "ExePath";
    internal const string InstalledProperty = "Installed";
    internal const string InstallPathProperty = "InstallPath";
    internal const string LauncherProperty = "Launcher";
    internal const string RevisionProperty = "Revision";

    private static readonly Version VersionInstance = new(1, 0);

    private readonly IFileSystem _fileSystem;
    private readonly string _registryGamePath;

    private IRegistryKey _baseKey;

    private bool _disposed;

    /// <inheritdoc/>
    public GameType Type { get; }

    /// <inheritdoc/>
    public bool Exits => AccessSubKey(string.Empty, _ => true);

    /// <inheritdoc/>
    public Version? Version => AccessSubKey(VersionKey, _ => VersionInstance);


    private T? AccessSubKey<T>(string subKey, Func<IRegistryKey, T> versionKeyAction)
    {
        using var versionKey = GetOrOpenGameKey()?.OpenSubKey(subKey);
        return versionKey is null ? default : versionKeyAction(versionKey);
    }

    /// <inheritdoc/>
    public string? CdKey => AccessSubKey(VersionKey, vKey => vKey.GetValue<string>(CDKeyProperty));

    /// <inheritdoc/>
    public int? EaWGold => AccessSubKey(VersionKey, vKey => vKey.GetValue<int?>(EawGoldProperty));

    /// <inheritdoc/>
    public IFileInfo? ExePath
    {
        get
        {
            return AccessSubKey(VersionKey, vKey =>
            {
                var exePath = vKey.GetValue<string?>(ExePathProperty);
                return exePath is not null ? _fileSystem.FileInfo.New(exePath) : null;
            });
        }
    }

    /// <inheritdoc/>
    public bool? Installed => AccessSubKey<bool?>(VersionKey, vKey =>
    {
        var value = vKey.GetValue<int?>(InstalledProperty);
        return value == 1;
    });

    /// <inheritdoc/>
    public IDirectoryInfo? InstallPath
    {
        get
        {
            return AccessSubKey(VersionKey, vKey =>
            {
                var installPath = vKey.GetValue<string?>(InstallPathProperty);
                return installPath is not null ? _fileSystem.DirectoryInfo.New(installPath) : null;
            });
        }
    }

    /// <inheritdoc/>
    public IFileInfo? Launcher
    {
        get
        {
            return AccessSubKey(VersionKey, vKey =>
            {
                var launcherPath = vKey.GetValue<string?>(LauncherProperty);
                return launcherPath is not null ? _fileSystem.FileInfo.New(launcherPath) : null;
            });
        }
    }

    /// <inheritdoc/>
    public int? Revision => AccessSubKey(VersionKey, vKey => vKey.GetValue<int?>(RevisionProperty));

    public GameRegistry(GameType gameType, IRegistryKey baseKey, string registryGamePath, IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        if (baseKey is null)
            throw new ArgumentNullException(nameof(baseKey));
        if (baseKey.View is not RegistryView.Registry32)
            throw new ArgumentOutOfRangeException(nameof(baseKey), "The base key have a 32bit registry view");
        if (!baseKey.Name.Equals("HKEY_LOCAL_MACHINE"))
            throw new ArgumentOutOfRangeException(nameof(baseKey), "The base key must be based on HKEY_LOCAL_MACHINE");
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(registryGamePath);

        Type = gameType;
        _baseKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));
        _registryGamePath = registryGamePath;
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        GetOrOpenGameKey();
    }

    /// <inheritdoc/>
    ~GameRegistry()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposed all managed resources acquired by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private IRegistryKey? GetOrOpenGameKey()
    {
        ThrowIfDisposed();
        return _baseKey.OpenSubKey(_registryGamePath);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _disposed = true;
            _baseKey.Dispose();
            _baseKey = null!;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(ToString());
    }
}