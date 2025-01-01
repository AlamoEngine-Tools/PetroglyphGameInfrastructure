using System;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients;

public class GameClientFactoryTest : CommonTestBase
{
    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        PetroglyphGameInfrastructure.InitializeServices(sc);
    }

    [Fact]
    public void CreateClient_NullArgs_Throws()
    {
        var factory = new GameClientFactory(ServiceProvider);
        Assert.Throws<ArgumentNullException>(() => factory.CreateClient(null!));
    }

    [Fact]
    public void CreateClient_CorrectTypeAndSetsProperty()
    {
        var factory = new GameClientFactory(ServiceProvider);
        var game = CreateRandomGame();

        var client = factory.CreateClient(game);
        Assert.IsType<PetroglyphStarWarsGameClient>(client);
        Assert.Same(game, client.Game);
    }
}