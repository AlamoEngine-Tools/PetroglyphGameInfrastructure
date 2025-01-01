using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamRegistryFactoryTest
{
    private readonly IRegistry _registry = new InMemoryRegistry();
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;

    public SteamRegistryFactoryTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton(_registry);
        _serviceProvider = sc.BuildServiceProvider();
    }

    [Fact]
    public void Test_CreateWrapper()
    {
        var factory = new SteamRegistryFactory(_serviceProvider);

        var registry = factory.CreateRegistry();

        Type? expectedType = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            expectedType = typeof(WindowsSteamRegistry);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            expectedType = typeof(LinuxSteamRegistry);

        if (expectedType is null)
            Assert.Fail("Platform was not supported");

        Assert.IsType(expectedType, registry);
    }
}