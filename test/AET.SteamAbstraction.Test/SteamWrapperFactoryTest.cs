using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperFactoryTest
{
    private readonly IServiceProvider _serviceProvider;

    public SteamWrapperFactoryTest()
    {
        var sc = new ServiceCollection();
        var registry = new InMemoryRegistry();
        var fs = new MockFileSystem();
        
        sc.AddSingleton<IFileSystem>(fs);
        sc.AddSingleton<IRegistry>(registry);
        SteamAbstractionLayer.InitializeServices(sc);

        _serviceProvider = sc.BuildServiceProvider();
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void Test_CreateWrapper_Windows()
    {
        var factory = new SteamWrapperFactory(_serviceProvider);
        var wrapper = factory.CreateWrapper();
        Assert.IsType<WindowsSteamWrapper>(wrapper);
    }
}