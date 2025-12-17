using System;
using Microsoft.Extensions.DependencyInjection;

namespace AET.Testing;

public abstract class TestBaseWithServiceProvider
{
    protected readonly IServiceProvider ServiceProvider;

    protected TestBaseWithServiceProvider()
    {
        var sc = new ServiceCollection();
        // ReSharper disable once VirtualMemberCallInConstructor
        SetupServices(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    protected virtual void SetupServices(IServiceCollection serviceCollection)
    {
    }
}