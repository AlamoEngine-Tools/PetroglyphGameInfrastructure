#if Windows // TODO: Enable for linux

using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Test.Steam;

public class IGameClientFactoryTest : CommonTestBase
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        sc.AddSingleton(_registry);
        SteamAbstractionLayer.InitializeServices(sc);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        SteamPetroglyphStarWarsGameClients.InitializeServices(sc);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void CreateClient_CreatesCorrectClientType(GameIdentity identity)
    {
        var game = FileSystem.InstallGame(identity, ServiceProvider);
        var factory = ServiceProvider.GetRequiredService<IGameClientFactory>();
        var client = factory.CreateClient(game);

        var expectedClientType = typeof(PetroglyphStarWarsGameClient);
        if (game.Platform is GamePlatform.SteamGold)
            expectedClientType = typeof(SteamPetroglyphStarWarsGameClient);

        Assert.IsType(expectedClientType, client);
    }
}

#endif