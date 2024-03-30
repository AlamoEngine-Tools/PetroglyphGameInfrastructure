using AnakinRaW.CommonUtilities.Registry;
using System.Collections.Generic;
using System.IO.Abstractions;
using System;
using System.Linq;
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
            return _registryKey!.GetKey(SteamProcessNode);
        }
    }

    int? ISteamRegistry.ActiveUserId
    {
        get
        {
            ThrowIfDisposed();
            return !_registryKey!.GetValue(SteamActiveUserKey, SteamProcessNode, out int? userId) ? null : userId;
        }
        set
        {
            ThrowIfDisposed();
            value ??= 0;
            _registryKey!.WriteValue(SteamActiveUserKey, SteamProcessNode, value);
        }
    }

    public int? ProcessId
    {
        get
        {
            ThrowIfDisposed();
            return !_registryKey!.GetValue(SteamProcessIdKey, SteamProcessNode, out int? pid) ? null : pid;
        }
    }

    public IFileInfo? ExecutableFile
    {
        get
        {
            ThrowIfDisposed();
            return !_registryKey!.GetValue(SteamExeKey, out string? filePath)
                ? null
                : _fileSystem.FileInfo.New(filePath!);
        }
    }

    public IDirectoryInfo? InstallationDirectory
    {
        get
        {
            ThrowIfDisposed();
            return !_registryKey!.GetValue(SteamPathKey, out string? path)
                ? null
                : _fileSystem.DirectoryInfo.New(path!);
        }
    }

    public ISet<uint>? InstalledApps
    {
        get
        {
            ThrowIfDisposed();
            var keyNames = _registryKey!.GetSubKeyNames(SteamAppsNode);
            if (keyNames is null)
                return null;
            var ids = keyNames
                .Select(n => !uint.TryParse(n, out var id) ? (uint?)0 : id)
                .OfType<uint>();
            return new HashSet<uint>(ids);
        }
    }


    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        _registryKey?.Dispose();
        _registryKey = null;
    }
}