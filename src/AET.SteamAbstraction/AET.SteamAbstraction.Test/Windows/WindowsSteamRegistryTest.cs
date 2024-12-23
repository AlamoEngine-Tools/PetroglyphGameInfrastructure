#if Windows

using System.Collections.Generic;
using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test.Windows;

public class WindowsSteamRegistryTest : SteamRegistryTestBase
{
    protected readonly IRegistry InternalRegistry = new InMemoryRegistry();

    protected override void BuildServiceCollection(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(InternalRegistry);
    }

    internal WindowsSteamRegistry CreateWindowsRegistry(bool steamExists = true)
    {
        var registry = new WindowsSteamRegistry(ServiceProvider);

        if (steamExists)
        {
            using var hkcu = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            using var steamKey = hkcu.CreateSubKey("Software\\Valve\\Steam");

            steamKey.SetValue("SteamExe", FileSystem.Path.GetFullPath(SteamExePath));
            steamKey.SetValue("SteamPath", FileSystem.Path.GetFullPath(SteamInstallPath));
        }

        return registry;
    }

    private protected override ISteamRegistry CreateRegistry(bool steamExists = true)
    {
        return CreateWindowsRegistry(steamExists);
    }

    protected override void SetSteamPid(int pid)
    {
        using var hkcu = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        using var steamKey = hkcu.CreateSubKey("Software\\Valve\\Steam\\ActiveProcess");
        steamKey.SetValue("pid", pid);
    }

    [Fact]
    public void DefaultPropertyValues()
    {
        var registry = CreateWindowsRegistry(false);

        Assert.Null(registry.ProcessId);
        Assert.Null(registry.ExecutableFile);
        Assert.Null(registry.InstallationDirectory);

        Assert.Null(registry.ActiveUserId);
        Assert.Null(registry.ActiveProcessKey);
        Assert.Empty(registry.InstalledApps);

        InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("Software\\Valve\\Steam\\ActiveProcess");
        Assert.Null(registry.ActiveUserId);
    }

    [Fact]
    public void Write_ActiveUserId()
    {
        var registry = CreateWindowsRegistry();

        registry.ActiveUserId = 123;

        var actualValue = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .OpenSubKey("Software\\Valve\\Steam\\ActiveProcess")?.GetValue("ActiveUser");

        Assert.Equal(123, actualValue);
    }

    [Fact]
    public void GetSteamApps()
    {
        var registry = CreateWindowsRegistry();

        using var _ = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("Software\\Valve\\Steam\\Apps\\123", false);
        using var __ = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("Software\\Valve\\Steam\\Apps\\456", false);
        using var ___ = InternalRegistry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("Software\\Valve\\Steam\\Apps\\789", false);

        var apps = registry.InstalledApps;

        Assert.Equivalent(new HashSet<uint>{123, 456, 789}, apps, true);
    }
}

#endif