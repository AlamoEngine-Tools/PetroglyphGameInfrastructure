using System;
using System.IO.Abstractions;
using System.Reflection;
using AET.SteamAbstraction.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public abstract class SteamRegistryTestBase
{
    protected readonly MockFileSystem FileSystem = new();

    protected readonly string SteamInstallPath = "steamDir";
    protected readonly string SteamExePath = "steamDir/steam";

    protected readonly IServiceProvider ServiceProvider;

    protected SteamRegistryTestBase()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(FileSystem);
        SteamAbstractionLayer.InitializeServices(sc);
        // ReSharper disable once VirtualMemberCallInConstructor
        BuildServiceCollection(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    private protected abstract ISteamRegistry CreateRegistry(bool steamExists = true);

    protected abstract void SetSteamPid(int pid);

    protected virtual void BuildServiceCollection(IServiceCollection serviceCollection)
    {
    }

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

        Assert.Equal(FileSystem.FileInfo.New(SteamExePath).FullName, registry.ExecutableFile.FullName);
        Assert.Equal(FileSystem.DirectoryInfo.New(SteamInstallPath).FullName, registry.InstallationDirectory.FullName);
        Assert.True(registry.ProcessId is null or 0);
    }

    [Fact]
    public void TestInstallationProperties_SteamInstalledAndRunning()
    {
        var registry = CreateRegistry();

        Assert.Equal(FileSystem.FileInfo.New(SteamExePath).FullName, registry.ExecutableFile.FullName);
        Assert.Equal(FileSystem.DirectoryInfo.New(SteamInstallPath).FullName, registry.InstallationDirectory.FullName);

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