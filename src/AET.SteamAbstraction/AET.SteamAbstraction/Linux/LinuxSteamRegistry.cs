using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities;
using AnakinRaW.CommonUtilities.Registry;

namespace AET.SteamAbstraction;

internal class LinuxSteamRegistry : DisposableObject, ISteamRegistry
{
    private readonly IServiceProvider _serviceProvider;

    public LinuxSteamRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IRegistryKey? ActiveProcessKey { get; }

    public int? ActiveUserId { get; set; }

    public int? ProcessId { get; }

    public IFileInfo? ExecutableFile { get; }

    public IDirectoryInfo? InstallationDirectory { get; }

    public ISet<uint>? InstalledApps { get; }
}