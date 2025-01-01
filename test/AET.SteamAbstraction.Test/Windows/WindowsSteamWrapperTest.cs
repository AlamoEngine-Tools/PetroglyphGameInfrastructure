#if Windows

using System;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test.Windows;

public class WindowsSteamWrapperTest : SteamWrapperTestBase
{
    protected readonly IRegistry InternalRegistry = new InMemoryRegistry();

    protected override void BuildServiceCollection(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(InternalRegistry);
    }

    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(new WindowsSteamRegistry(ServiceProvider), null!));
    }
}

#endif