using System;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients;

public class GameClientFactoryTest : GameInfrastructureTestBase
{
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
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
        var game = GetOrCreateGameInstallation().Game;

        var client = factory.CreateClient(game);
        Assert.IsType<PetroglyphStarWarsGameClient>(client);
        Assert.Same(game, client.Game);
    }
}