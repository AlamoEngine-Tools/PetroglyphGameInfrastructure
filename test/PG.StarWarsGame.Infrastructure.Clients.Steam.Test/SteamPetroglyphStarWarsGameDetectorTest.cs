#if Windows // TODO: Enable for linux

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Testing;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Test;

[SupportedOSPlatform("windows")]
public class SteamPetroglyphStarWarsGameDetectorTest : GameDetectorTestBase<object>
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    protected override bool SupportInitialization => true;
    protected override ICollection<GamePlatform> SupportedPlatforms => [GamePlatform.SteamGold];
    protected override bool CanDisableInitRequest => false;

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(_registry);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
        SteamPetroglyphStarWarsGameClients.InitializeServices(serviceCollection);
    }

    protected override IGameDetector CreateDetector(GameDetectorTestInfo<object> gameInfo, bool shallHandleInitialization)
    {
        return new SteamPetroglyphStarWarsGameDetector(ServiceProvider);
    }

    protected override GameDetectorTestInfo<object> SetupGame(GameIdentity gameIdentity)
    {
        return SetupGame(gameIdentity, (game, otherGameType) =>
        {
            GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(game);
            GameInfrastructureTesting.Registry(ServiceProvider).CreateNonExistingRegistry(otherGameType);
        });
    }

    protected override GameDetectorTestInfo<object> SetupForRequiredInitialization(GameIdentity gameIdentity)
    {
        return SetupGame(gameIdentity, (game, otherGameType) =>
        {
            GameInfrastructureTesting.Registry(ServiceProvider).CreateFrom(TestGameRegistrySetupData.Uninitialized(game.Type));
            GameInfrastructureTesting.Registry(ServiceProvider).CreateNonExistingRegistry(otherGameType);
        });
    }

    protected override void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<object> info)
    {
        if (!shallInitSuccessfully)
            return;
        var registrySetupData = TestGameRegistrySetupData.Installed(info.GameType, info.GameDirectory!);
        GameInfrastructureTesting.Registry(ServiceProvider).CreateFrom(registrySetupData);
    }

    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(null!));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_SteamNotInstalled_ShouldReturnNotInstalled(GameType gameType)
    {
        var gameId = new GameIdentity(gameType, GamePlatform.SteamGold);
        var expected = GameDetectionResult.NotInstalled(gameId.Type);

        GetOrCreateGameInstallation(gameId);

        var detector = new SteamPetroglyphStarWarsGameDetector(ServiceProvider);
        var result = detector.Detect(gameType, GamePlatform.SteamGold);

        expected.AssertEqual(result);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_SteamInstalledButGameNotRegisteredToSteam_ShouldReturnNotInstalled(GameType gameType)
    {
        var gameId = new GameIdentity(gameType, GamePlatform.SteamGold);
        var expected = GameDetectionResult.NotInstalled(gameId.Type);

        GetOrCreateGameInstallation(gameId);

        var detector = new SteamPetroglyphStarWarsGameDetector(ServiceProvider);
        var result = detector.Detect(gameType, GamePlatform.SteamGold);

        expected.AssertEqual(result);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_SteamInstalledButGameNotFullyInstalled_ShouldReturnNotInstalled(GameType gameType)
    {
        var gameId = new GameIdentity(gameType, GamePlatform.SteamGold);
        var expected = GameDetectionResult.NotInstalled(gameId.Type);

        var info = SetupGame(gameId, (game, otherGameType) =>
        {
            GameInfrastructureTesting.Registry(ServiceProvider).CreateInstalled(game);
            GameInfrastructureTesting.Registry(ServiceProvider).CreateNonExistingRegistry(otherGameType);
        }, SteamAppState.StateUpdateRequired);

        var detector = CreateDetector(info, true);
        var result = detector.Detect(gameType, GamePlatform.SteamGold);

        expected.AssertEqual(result);
    }

    private GameDetectorTestInfo<object> SetupGame(
        GameIdentity gameIdentity,
        Action<IGame, GameType> registrySetup,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        // Install Steam (regardless whether the identity is supported)
        var steam = SteamTesting.Steam(ServiceProvider);
        steam.Install();

        if (gameIdentity.Platform != GamePlatform.SteamGold)
            return new GameDetectorTestInfo<object>(gameIdentity.Type, null, null);

        // Install Game
        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        // Register Game to Steam
        var lib = steam.InstallDefaultLibrary();
        IList<uint> depots = gameIdentity.Type == GameType.Foc ? [32472] : [];
        lib.InstallGame(32470, "Star Wars Empire at War", depots, appState);

        // To Registry
        var otherGameType = gameIdentity.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw;
        registrySetup(game, otherGameType);

        return new GameDetectorTestInfo<object>(gameIdentity.Type, game.Directory, null);
    }
}

#endif