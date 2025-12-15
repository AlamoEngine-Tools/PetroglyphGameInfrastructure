using System;
using System.IO.Abstractions;
using AET.SteamAbstraction.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Testing;

internal class TestingSteamRegistryImpl(IFileSystem fileSystem, IServiceProvider serviceProvider) : ITestingSteamRegistry
{
    private readonly ISteamRegistry _registry = serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();

    public IFileInfo? ExecutableFile => _registry.ExecutableFile;
    public IDirectoryInfo? InstallationDirectory => _registry.InstallationDirectory;
    public int? ProcessId => _registry.ProcessId;

    public void InstallSteam()
    {
        if (_registry is WindowsSteamRegistry)
            InstallWindowsRegistry();
        else if (_registry is LinuxSteamRegistry)
            InstallLinuxRegistry();
    }
    

    public void SetPid(int? pid)
    {
        if (_registry is WindowsSteamRegistry)
            SetPidWindowsRegistry(pid);
        else if (_registry is LinuxSteamRegistry)
            SetPidLinuxRegistry(pid);
    }

    public void SetUserId(uint userId)
    {
        if (_registry is WindowsSteamRegistry)
            SetUserIdWindowsRegistry(userId);
        // Does not exist for Linux
    }

    private void InstallWindowsRegistry()
    {
        using var key = _registry.OpenSteamRegistryKey();
        Assert.NotNull(key);
        key.SetValue("SteamExe", fileSystem.Path.GetFullPath(TestingSteamConstants.SteamExePath));
        key.SetValue("SteamPath", fileSystem.Path.GetFullPath(TestingSteamConstants.SteamInstallPath));
    }

    private void SetPidWindowsRegistry(int? pid)
    {
        if (pid is null)
            return;
        using var key = _registry.OpenSteamRegistryKey();
        Assert.NotNull(key);
        using var activeProcessKey = key.CreateSubKey("ActiveProcess");

        activeProcessKey!.SetValue("pid", pid);
    }

    private void SetUserIdWindowsRegistry(uint userId)
    {
        using var key = _registry.OpenSteamRegistryKey();
        Assert.NotNull(key);
        using var activeProcessKey = key.CreateSubKey("ActiveProcess");

        activeProcessKey!.SetValue("ActiveUser", userId);
    }

    private void InstallLinuxRegistry()
    {
        throw new NotImplementedException("Linux is currently not supported");
    }

    private void SetPidLinuxRegistry(int? pid)
    {
        throw new NotImplementedException("Linux is currently not supported");
    }

    public void Dispose()
    {
        _registry.Dispose();
    }
}