using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Utilities;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamRegistryFactoryTest
{
    [Fact]
    public void Test_CreateWrapper()
    {
        var reg = new Mock<IRegistry>();
        reg.Setup(r => r.OpenBaseKey(It.IsAny<RegistryHive>(), It.IsAny<RegistryView>()))
            .Returns(new Mock<IRegistryKey>().Object);

        var sc = new ServiceCollection();
        sc.AddSingleton(_ => reg.Object);
        sc.AddSingleton(new Mock<IProcessHelper>().Object);
        sc.AddSingleton<IFileSystem>(new MockFileSystem());
        var factory = new SteamRegistryFactory(sc.BuildServiceProvider());

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