using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

public static class GameInfrastructureTesting
{
    public static ITestingGameInstallation Game(IGameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        return new TestingGameInstallationImpl(gameIdentity, serviceProvider);
    }

    public static ITestingGameRegistry Registry(IServiceProvider serviceProvider)
    {
        return new TestingGameRegistryImpl(serviceProvider);
    }
}