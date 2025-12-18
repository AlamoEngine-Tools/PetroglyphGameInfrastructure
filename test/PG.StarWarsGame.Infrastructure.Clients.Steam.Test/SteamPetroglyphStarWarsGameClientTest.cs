#if Windows // TODO: Enable for linux

using AET.SteamAbstraction;
using AET.SteamAbstraction.Testing;
using AET.SteamAbstraction.Utilities;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Test.Clients;
using PG.StarWarsGame.Infrastructure.Testing;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using AET.Testing.Extensions;
using Xunit;
using PG.StarWarsGame.Infrastructure.Testing.Game;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Test;

[SupportedOSPlatform("windows")]
public class SteamPetroglyphStarWarsGameClientTest : PetroglyphStarWarsGameClientTest
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    private ISteamFakeProcess? _steamProcess;

    protected override ICollection<GamePlatform> SupportedPlatforms { get; } = [GamePlatform.SteamGold];

    protected override IGame GetOrÍnstallGame(IGameIdentity? identity = null)
    {
        if (GameInstallation.Game is not null)
            return GameInstallation.Game;
        var type = identity?.Type ?? Random.Enum<GameType>();
        var steamIdentity = new GameIdentity(type, GamePlatform.SteamGold);
        return GameInstallation.Install(steamIdentity);
    }

    protected override void BeforePlay()
    {
        // Install Steam (regardless whether the identity is supported)
        var steam = SteamTesting.Steam(ServiceProvider);
        steam.Install();
        _steamProcess = steam.FakeStart(12345);
        base.BeforePlay();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(_registry);
        base.SetupServices(serviceCollection);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
        SteamPetroglyphStarWarsGameClients.InitializeServices(serviceCollection);

        serviceCollection.AddSingleton<TestProcessHelper>(sp => new TestProcessHelper(sp));
        serviceCollection.AddSingleton<IProcessHelper>(sp => sp.GetRequiredService<TestProcessHelper>());
        
    }

    [Fact]
    public void Ctor_SteamClient_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameClient(null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameClient(GetOrÍnstallGame(), null!));
    }

    [Fact]
    public void SteamPetroglyphStarWarsGameClient_Lifecycle()
    {
        var client = new SteamPetroglyphStarWarsGameClient(GetOrÍnstallGame(), ServiceProvider);
        Assert.NotNull(client.SteamWrapper);

        client.Dispose();
        Assert.Throws<ObjectDisposedException>(client.Play);
        Assert.Throws<ObjectDisposedException>(client.SteamWrapper.StartSteam);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Ctor_NonSteamGameThrows(GameIdentity identity)
    {
        if (identity.Platform is GamePlatform.SteamGold)
            return;
        var otherGameInstallation = GameInfrastructureTesting.Game(ServiceProvider);
        var otherGame = otherGameInstallation.Install(identity);
        Assert.Throws<ArgumentException>(() => new SteamPetroglyphStarWarsGameClient(otherGame, ServiceProvider));
    }

    [Fact]
    public void Play_SteamNotInstalled_Throws()
    {
        var game = GetOrÍnstallGame();
        var client = new SteamPetroglyphStarWarsGameClient(game, ServiceProvider);
        Assert.Throws<GameStartException>(client.Play);

        var expected = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);

        TestPlay(game, expected, gameClient => gameClient.Play(), shallThrowGameStartException: true);
    }

    [Fact]
    public void Play_SteamNotRunning_Throws()
    {
        // Install Steam (regardless whether the identity is supported)
        SteamTesting.Steam(ServiceProvider).Install();

        var game = GetOrÍnstallGame();
        var expected = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);

        TestPlay(game, expected, gameClient => gameClient.Play(), shallThrowGameStartException: true);
    }

    public override void Dispose()
    {
        base.Dispose();
        _steamProcess?.Kill();
    }
}

#endif