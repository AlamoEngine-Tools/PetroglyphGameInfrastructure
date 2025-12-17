using System;

namespace AET.SteamAbstraction.Testing;

public static class SteamTesting
{
    public static ITestingSteamInstallation Steam(IServiceProvider serviceProvider)
    {
        return new TestingSteamInstallationImpl(serviceProvider);
    }

    public static ITestingSteamRegistry SteamRegistry(IServiceProvider serviceProvider)
    {
        return new TestingSteamRegistryImpl(serviceProvider);
    }
}