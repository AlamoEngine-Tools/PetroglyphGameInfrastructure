using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperFactoryTest
{
    [Fact]
    public void Test_CreateWrapper()
    {
        var regFactory = new Mock<ISteamRegistryFactory>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            regFactory.Setup(f => f.CreateRegistry()).Returns(new Mock<IWindowsSteamRegistry>().Object);
        else 
            regFactory.Setup(f => f.CreateRegistry()).Returns(new Mock<ISteamRegistry>().Object);

        var sc = new ServiceCollection();
        sc.AddSingleton(_ => regFactory.Object);
        sc.AddSingleton(new Mock<IProcessHelper>().Object);
        sc.AddSingleton<IFileSystem>(new MockFileSystem());
        var factory = new SteamWrapperFactory(sc.BuildServiceProvider());

        var wrapper = factory.CreateWrapper();

        Type? expectedType = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            expectedType = typeof(WindowsSteamWrapper);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            expectedType = typeof(LinuxSteamWrapper);

        if (expectedType is null)
            Assert.Fail("Platform was not supported");

        Assert.IsType(expectedType, wrapper);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void Test_CreateWrapper_ThrowsWrongType_Windows()
    {
        var regFactory = new Mock<ISteamRegistryFactory>();

        regFactory.Setup(f => f.CreateRegistry()).Returns(new Mock<ISteamRegistry>().Object);

        var sc = new ServiceCollection();
        sc.AddSingleton(_ => regFactory.Object);
        sc.AddSingleton(new Mock<IProcessHelper>().Object);
        sc.AddSingleton<IFileSystem>(new MockFileSystem());
        var factory = new SteamWrapperFactory(sc.BuildServiceProvider());

        Assert.Throws<InvalidOperationException>(() => factory.CreateWrapper());
    }
}