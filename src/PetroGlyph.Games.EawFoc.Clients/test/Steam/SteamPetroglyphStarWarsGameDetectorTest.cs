using System;
using System.Collections.Generic;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Testing.Installation;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Steam;

public class SteamPetroglyphStarWarsGameDetectorTest : GameDetectorTestBase<EmptyStruct>
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    protected override bool SupportInitialization => true;
    protected override ICollection<GamePlatform> SupportedPlatforms => [GamePlatform.SteamGold];
    protected override bool CanDisableInitRequest => false;

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        sc.AddSingleton(_registry);
        SteamAbstractionLayer.InitializeServices(sc);
        PetroglyphGameClients.InitializeServices(sc);
    }

    protected override IGameDetector CreateDetector(GameDetectorTestInfo<EmptyStruct> gameInfo, bool shallHandleInitialization)
    {
        return new SteamPetroglyphStarWarsGameDetector(ServiceProvider);
    }

    protected override GameDetectorTestInfo<EmptyStruct> SetupGame(GameIdentity gameIdentity)
    {
        return SetupGame(gameIdentity, (game, otherGameType) =>
        {
            TestGameRegistrySetupData.Installed(game.Type, game.Directory).Create(ServiceProvider);
            otherGameType.CreateNonExistingRegistry(ServiceProvider);
        });
    }

    protected override GameDetectorTestInfo<EmptyStruct> SetupForRequiredInitialization(GameIdentity gameIdentity)
    {
        return SetupGame(gameIdentity, (game, otherGameType) =>
        {
            TestGameRegistrySetupData.Uninitialized(game.Type).Create(ServiceProvider);
            otherGameType.CreateNonExistingRegistry(ServiceProvider);
        });
    }

    protected override void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<EmptyStruct> info)
    {
        if (!shallInitSuccessfully)
            return;
        var registrySetupData = TestGameRegistrySetupData.Installed(info.GameType, info.GameDirectory!);
        registrySetupData.Create(ServiceProvider);
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

        FileSystem.InstallGame(gameId, ServiceProvider);

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

        FileSystem.InstallGame(gameId, ServiceProvider);

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
            TestGameRegistrySetupData.Installed(game.Type, game.Directory).Create(ServiceProvider);
            otherGameType.CreateNonExistingRegistry(ServiceProvider);
        }, SteamAppState.StateUpdateRequired);

        var detector = CreateDetector(info, true);
        var result = detector.Detect(gameType, GamePlatform.SteamGold);

        expected.AssertEqual(result);
    }

    private GameDetectorTestInfo<EmptyStruct> SetupGame(
        GameIdentity gameIdentity, 
        Action<IGame, GameType> registrySetup, 
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        // Install Steam (regardless whether the identity is supported)
        var registry = ServiceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        FileSystem.InstallSteam(registry);

        if (gameIdentity.Platform != GamePlatform.SteamGold)
            return new GameDetectorTestInfo<EmptyStruct>(gameIdentity.Type, null, default);

        // Install Game
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        // Register Game to Steam
        var lib = FileSystem.InstallDefaultLibrary(ServiceProvider);
        IList<uint> depots = gameIdentity.Type == GameType.Foc ? [32472] : [];
        lib.InstallGame(32470, "Star Wars Empire at War", depots, appState);

        // To Registry
        var otherGameType = gameIdentity.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw;
        registrySetup(game, otherGameType);

        return new GameDetectorTestInfo<EmptyStruct>(gameIdentity.Type, game.Directory, default);
    }
}