using System;
using System.IO.Abstractions;
using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities;

namespace AET.SteamAbstraction;

internal class LinuxSteamRegistry(IServiceProvider serviceProvider) : DisposableObject, ISteamRegistry
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public IFileInfo? ExecutableFile { get; }

    public IDirectoryInfo? InstallationDirectory { get; }

    public int? ProcessId { get; }
}