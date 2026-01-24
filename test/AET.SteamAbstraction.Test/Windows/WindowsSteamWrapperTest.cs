#if Windows
using System;
using Xunit;

namespace AET.SteamAbstraction.Test.Windows;

public class WindowsSteamWrapperTest : SteamWrapperTestBase
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(new WindowsSteamRegistry(ServiceProvider), null!));
    }
}

#endif