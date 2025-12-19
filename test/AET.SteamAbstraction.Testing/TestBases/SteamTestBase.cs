using System;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction.Testing.TestBases;

public abstract class SteamTestBase : TestBaseWithServiceProvider, IDisposable
{
    protected readonly ITestingSteamInstallation Steam;
    
    protected SteamTestBase()
    {
        Steam = SteamTesting.Steam(ServiceProvider);
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
    }

    public virtual void Dispose()
    {
        Steam.Dispose();
    }
}