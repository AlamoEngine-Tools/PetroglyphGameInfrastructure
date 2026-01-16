using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Testing;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Testing;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public abstract class SteamRegistryTestBase : TestBaseWithServiceProvider
{
    protected readonly MockFileSystem FileSystem = new();
    protected readonly IRegistry InternalRegistry = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
        ? new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike) 
        : new InMemoryRegistry();

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(FileSystem);
        serviceCollection.AddSingleton(InternalRegistry);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
    }

    private protected abstract ISteamRegistry CreateRegistry(bool steamExists = true);

    protected abstract void SetSteamPid(int pid);

    [Fact]
    public void TestInstallationProperties_SteamNotInstalled()
    {
        var registry = CreateRegistry(false);

        Assert.Null(registry.ExecutableFile);
        Assert.Null(registry.InstallationDirectory);
        Assert.Null(registry.ProcessId);
    }

    [Fact]
    public void TestInstallationProperties_SteamInstalled()
    {
        var registry = CreateRegistry();

        Assert.Equal(FileSystem.FileInfo.New(TestingSteamConstants.SteamExePath).FullName, registry.ExecutableFile!.FullName);
        Assert.Equal(FileSystem.DirectoryInfo.New(TestingSteamConstants.SteamInstallPath).FullName, registry.InstallationDirectory!.FullName);
        Assert.True(registry.ProcessId is null or 0);
    }

    [Fact]
    public void TestInstallationProperties_SteamInstalledAndRunning()
    {
        var registry = CreateRegistry();

        Assert.Equal(FileSystem.FileInfo.New(TestingSteamConstants.SteamExePath).FullName, registry.ExecutableFile!.FullName);
        Assert.Equal(FileSystem.DirectoryInfo.New(TestingSteamConstants.SteamInstallPath).FullName, registry.InstallationDirectory!.FullName);

        SetSteamPid(1234);

        Assert.Equal(1234, registry.ProcessId);
    }

    [Fact]
    public void AccessPropertyWhenDisposed_Throws()
    {
        var registry = CreateRegistry();

        var properties = registry.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        registry.Dispose();

        foreach (var propertyInfo in properties)
        {
            if (propertyInfo.Name == "IsDisposed")
                continue;
            var e = Record.Exception(() => propertyInfo.GetValue(registry));
            if (e is null)
                Assert.Fail($"Expected Exception {typeof(ObjectDisposedException)} but none was thrown");
            if (e is not ObjectDisposedException && e.InnerException is not ObjectDisposedException) 
                Assert.Fail($"Expected Exception {typeof(ObjectDisposedException)} but got {e.GetType()}");
        }
    }
}