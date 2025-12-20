using System;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction.Testing.TestBases;

/// <summary>
/// Provides a base class for integration tests that require access to a testing Steam installation and related
/// services.
/// </summary>
/// <remarks>
/// Inheritors can use the protected <see cref="ITestingSteamInstallation"/> instance to interact with a
/// test Steam environment. The class ensures that required Steam services are initialized and disposed
/// appropriately.
/// </remarks>
public abstract class SteamTestBase : TestBaseWithServiceProvider, IDisposable
{
    /// <summary>
    /// Gets the testing Steam installation instance for use in tests.
    /// </summary>
    protected readonly ITestingSteamInstallation Steam;
    
    /// <summary>
    /// Initializes a new instance of the SteamTestBase class for use in derived test classes.
    /// </summary>
    /// <remarks>
    /// This protected constructor is intended to be called by subclasses to set up the Steam testing
    /// environment. It initializes the Steam property using the provided service provider.
    /// </remarks>
    protected SteamTestBase()
    {
        Steam = SteamTesting.Steam(ServiceProvider);
    }

    /// <inheritdoc />
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        Steam.Dispose();
    }
}