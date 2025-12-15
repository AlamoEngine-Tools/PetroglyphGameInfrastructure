using System;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Testing;

internal sealed partial class TestingSteamInstallationImpl(IFileSystem fileSystem, IServiceProvider serviceProvider) : ITestingSteamInstallation
{
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public IDirectoryInfo? InstallationDirectory => Registry.InstallationDirectory;
    public ITestingSteamRegistry Registry { get; } = new TestingSteamRegistryImpl(fileSystem, serviceProvider);

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