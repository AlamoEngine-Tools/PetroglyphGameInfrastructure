using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperFactoryTest
{
    [Fact]
    public void Test_CreateWrapper()
    {
        var regFactory = new Mock<ISteamRegistryFactory>();
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
}