using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

public static class GameInfrastructureTesting
{
    public static ITestingGameInstallation Game(IGameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        return new TestingGameImpl(gameIdentity, serviceProvider);
    }

    public static ITestingGameRegistry Registry(IServiceProvider serviceProvider)
    {
        return new TestingGameRegistryImpl(serviceProvider);
    }
}