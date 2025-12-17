using System.IO.Abstractions;
using AET.Testing;
using AET.Testing.Attributes;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperFactoryTest : TestBaseWithServiceProvider
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(new MockFileSystem());
        serviceCollection.AddSingleton<IRegistry>(new InMemoryRegistry());
        SteamAbstractionLayer.InitializeServices(serviceCollection);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void Test_CreateWrapper_Windows()
    {
        var factory = new SteamWrapperFactory(ServiceProvider);
        var wrapper = factory.CreateWrapper();
        Assert.IsType<WindowsSteamWrapper>(wrapper);
    }
}