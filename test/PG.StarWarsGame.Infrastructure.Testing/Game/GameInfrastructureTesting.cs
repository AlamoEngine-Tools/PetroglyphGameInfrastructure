using System;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Game;

public static class GameInfrastructureTesting
{
    public static ITestingGameInstallation Game(IServiceProvider serviceProvider)
    {
        return new TestingGameImpl(serviceProvider);
    }

    public static ITestingGameRegistry Registry(IServiceProvider serviceProvider)
    {
        return new TestingGameRegistryImpl(serviceProvider);
    }
}