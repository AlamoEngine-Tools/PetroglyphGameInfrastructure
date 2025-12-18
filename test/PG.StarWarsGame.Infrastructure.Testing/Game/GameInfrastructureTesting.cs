using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Game;

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