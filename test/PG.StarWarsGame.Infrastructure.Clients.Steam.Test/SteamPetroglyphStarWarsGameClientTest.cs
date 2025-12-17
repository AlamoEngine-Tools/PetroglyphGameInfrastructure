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
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using System;
using System.Collections.Generic;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Test;

public class SteamPetroglyphStarWarsGameClientTest : PetroglyphStarWarsGameClientTest
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);
    private readonly PetroglyphStarWarsGame _game;

    private ISteamFakeProcess? _steamProcess;

    protected override ICollection<GamePlatform> SupportedPlatforms { get; } = [GamePlatform.SteamGold];

    protected override void BeforePlay()
    {
        // Install Steam (regardless whether the identity is supported)
        var steam = SteamTesting.Steam(ServiceProvider);
        steam.Install();
        _steamProcess = steam.FakeStart(12345);
        base.BeforePlay();
    }

    public SteamPetroglyphStarWarsGameClientTest()
    {
        _game = FileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
    }

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        sc.AddSingleton(_registry);
        base.SetupServiceProvider(sc);
        SteamAbstractionLayer.InitializeServices(sc);
        SteamPetroglyphStarWarsGameClients.InitializeServices(sc);

        sc.AddSingleton<TestProcessHelper>(sp => new TestProcessHelper(sp));
        sc.AddSingleton<IProcessHelper>(sp => sp.GetRequiredService<TestProcessHelper>());
        
    }

    [Fact]
    public void Ctor_SteamClient_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameClient(null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameClient(_game, null!));
    }

    [Fact]
    public void SteamPetroglyphStarWarsGameClient_Lifecycle()
    {
        var client = new SteamPetroglyphStarWarsGameClient(_game, ServiceProvider);
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
        var game = FileSystem.InstallGame(identity, ServiceProvider);
        Assert.Throws<ArgumentException>(() => new SteamPetroglyphStarWarsGameClient(game, ServiceProvider));
    }

    [Fact]
    public void Play_SteamNotInstalled_Throws()
    {
        var client = new SteamPetroglyphStarWarsGameClient(_game, ServiceProvider);
        Assert.Throws<GameStartException>(client.Play);

        var expected = new GameProcessInfo(_game, GameBuildType.Release, ArgumentCollection.Empty);

        TestPlay(_game, expected, gameClient => gameClient.Play(), shallThrowGameStartException: true);
    }

    [Fact]
    public void Play_SteamNotRunning_Throws()
    {
        // Install Steam (regardless whether the identity is supported)
        SteamTesting.Steam(ServiceProvider).Install();

        var expected = new GameProcessInfo(_game, GameBuildType.Release, ArgumentCollection.Empty);

        TestPlay(_game, expected, gameClient => gameClient.Play(), shallThrowGameStartException: true);
    }

    public override void Dispose()
    {
        base.Dispose();
        _steamProcess?.Kill();
    }
}

#endif