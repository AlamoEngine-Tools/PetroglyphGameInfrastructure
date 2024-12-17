using AnakinRaW.CommonUtilities.Registry;
using System.Collections.Generic;
using System.IO.Abstractions;
using System;
using System.Globalization;
using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction;

internal sealed class WindowsSteamRegistry(IServiceProvider serviceProvider) : DisposableObject, IWindowsSteamRegistry
{
    private const string SteamExeKey = "SteamExe";
    private const string SteamPathKey = "SteamPath";
    private const string SteamProcessIdKey = "pid";
    private const string SteamActiveUserKey = "ActiveUser";

    private const string SteamProcessNode = "ActiveProcess";
    private const string SteamAppsNode = "Apps";

    private IRegistryKey? _registryKey = serviceProvider.GetRequiredService<IRegistry>()
        .OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
        .CreateSubKey("Software\\Valve\\Steam");

    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public IRegistryKey? ActiveProcessKey
    {
        get
        {
            ThrowIfDisposed();
            return _registryKey!.OpenSubKey(SteamProcessNode);
        }
    }

    int? ISteamRegistry.ActiveUserId
    {
        get
        {
            return ReadFromSubKey(SteamProcessNode, key => key.GetValue<int?>(SteamActiveUserKey));
        }
        set
        {
            WriteToSubKey(SteamProcessNode, key =>
            {
                key.SetValue(SteamActiveUserKey, value ?? 0);
            });
        }
    }

    public int? ProcessId
    {
        get
        {
            return ReadFromSubKey(SteamProcessNode, key => key.GetValue<int?>(SteamProcessIdKey));
        }
    }

    public IFileInfo? ExecutableFile
    {
        get
        {
            ThrowIfDisposed();
            var path = _registryKey!.GetValue<string?>(SteamExeKey);
            return path == null ? null : _fileSystem.FileInfo.New(path);
        }
    }

    public IDirectoryInfo? InstallationDirectory
    {
        get
        {
            ThrowIfDisposed();
            var path = _registryKey!.GetValue<string?>(SteamPathKey);
            return path == null ? null : _fileSystem.DirectoryInfo.New(path);
        }
    }

    public ISet<uint>? InstalledApps
    {
        get
        {
            return ReadFromSubKey(SteamAppsNode, key =>
            {
                var appNames = key.GetSubKeyNames();
                var ids = new HashSet<uint>();
                foreach (var app in appNames)
                {
                    if (uint.TryParse(app, NumberStyles.None, null, out var id))
                        ids.Add(id);
                }
                return ids;
            });
        }
    }
    
    protected override void DisposeManagedResources()
    {
        _registryKey?.Dispose();
        _registryKey = null;
        base.DisposeManagedResources();
    }

    private T? ReadFromSubKey<T>(string subKeyName, Func<IRegistryKey, T> keyAction)
    {
        ThrowIfDisposed();
        using var subKey = _registryKey?.OpenSubKey(subKeyName);
        return subKey is null ? default : keyAction(subKey);
    }

    private void WriteToSubKey(string subKeyName, Action<IRegistryKey> keyAction)
    {
        ThrowIfDisposed();
        using var subKey = _registryKey?.OpenSubKey(subKeyName, true);
        if (subKey is not null)
            keyAction(subKey);
    }
}