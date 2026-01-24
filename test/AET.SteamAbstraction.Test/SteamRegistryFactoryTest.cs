using AET.SteamAbstraction.Registry;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Testing;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamRegistryFactoryTest : TestBaseWithServiceProvider
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(new MockFileSystem());
        serviceCollection.AddSingleton<IRegistry>(new InMemoryRegistry());
    }

    [Fact]
    public void Test_CreateWrapper()
    {
        var factory = new SteamRegistryFactory(ServiceProvider);

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