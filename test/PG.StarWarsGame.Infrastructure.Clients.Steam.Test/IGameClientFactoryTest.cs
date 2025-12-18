#if Windows // TODO: Enable for linux

using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Test;

public class IGameClientFactoryTest : GameInfrastructureTestBase
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(_registry);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
        SteamPetroglyphStarWarsGameClients.InitializeServices(serviceCollection);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreateClient_CreatesCorrectClientType(GameIdentity identity)
    {
        var game = GetOrÍnstallGame(identity);
        var factory = ServiceProvider.GetRequiredService<IGameClientFactory>();
        var client = factory.CreateClient(game);

        var expectedClientType = typeof(PetroglyphStarWarsGameClient);
        if (game.Platform is GamePlatform.SteamGold)
            expectedClientType = typeof(SteamPetroglyphStarWarsGameClient);

        Assert.IsType(expectedClientType, client);
    }
}

#endif