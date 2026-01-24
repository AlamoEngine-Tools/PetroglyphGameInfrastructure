// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

internal sealed partial class TestingSteamInstallationImpl(IServiceProvider serviceProvider) : ITestingSteamInstallation
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public IDirectoryInfo? InstallationDirectory => Registry.InstallationDirectory;

    public ITestingSteamRegistry Registry { get; } = SteamTesting.SteamRegistry(serviceProvider);

    [SupportedOSPlatform("windows")]
    public void Install()
    {
        Registry.InstallSteam();
        InstallSteamFilesOnly();
    }

    public void InstallSteamFilesOnly()
    {
        var configPath = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(TestingSteamConstants.SteamInstallPath, "config"));
        var exePath = _fileSystem.Path.GetFullPath(TestingSteamConstants.SteamExePath);

        _fileSystem.Directory.CreateDirectory(configPath);
        using var _ = _fileSystem.File.Create(exePath);
    }

    [SupportedOSPlatform("windows")]
    public ISteamFakeProcess FakeStart(int pid)
    {
        Registry.SetPid(pid);
        return new SteamFakeProcessImpl(_serviceProvider, pid);
    }

    public void Dispose()
    {
        Registry.Dispose();
    }
}