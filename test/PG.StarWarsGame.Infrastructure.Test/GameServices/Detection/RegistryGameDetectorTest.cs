﻿using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GameRegistryContainer(IGameRegistry? eawRegistry, IGameRegistry? focRegistry)
{
    public IGameRegistry? EawRegistry { get; } = eawRegistry;
    public IGameRegistry? FocRegistry { get; } = focRegistry;
}

public class RegistryGameDetectorTest : GameDetectorTestBase<GameRegistryContainer>
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    protected override bool SupportInitialization => true;
    protected override bool CanDisableInitRequest => true;
    protected override ICollection<GamePlatform> SupportedPlatforms => GITestUtilities.RealPlatforms;

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        sc.AddSingleton(_registry);
    }

    protected override IGameDetector CreateDetector(GameDetectorTestInfo<GameRegistryContainer> gameInfo, bool shallHandleInitialization)
    {
        var eawRegistry = gameInfo.DetectorSetupInfo?.EawRegistry 
                          ?? GameType.Eaw.CreateNonExistingRegistry(ServiceProvider);
        var focRegistry = gameInfo.DetectorSetupInfo?.FocRegistry 
                          ?? GameType.Foc.CreateNonExistingRegistry(ServiceProvider);
        return new RegistryGameDetector(eawRegistry, focRegistry, shallHandleInitialization, ServiceProvider);
    }

    private IGame InstallGame(GameIdentity gameIdentity)
    {
        return FileSystem.InstallGame(gameIdentity, ServiceProvider);
    }

    protected override GameDetectorTestInfo<GameRegistryContainer> SetupGame(GameIdentity gameIdentity)
    {
        var game = InstallGame(gameIdentity);
        var registryContainer = SetupRegistry(game.Type, TestGameRegistrySetupData.Installed(game.Type, game.Directory));
        return new GameDetectorTestInfo<GameRegistryContainer>(game.Type, game.Directory, registryContainer);
    }

    protected override GameDetectorTestInfo<GameRegistryContainer> SetupForRequiredInitialization(GameIdentity gameIdentity)
    {
        var game = InstallGame(gameIdentity);
        var registryContainer = SetupRegistry(game.Type, TestGameRegistrySetupData.Uninitialized(gameIdentity.Type));
        return new GameDetectorTestInfo<GameRegistryContainer>(game.Type, game.Directory, registryContainer);
    }

    private GameRegistryContainer SetupRegistry(GameType gameType, TestGameRegistrySetupData registrySetup)
    {
        var otherGameType = gameType == GameType.Eaw ? GameType.Foc : GameType.Eaw;

        var gameRegistry = registrySetup.Create(ServiceProvider);
        var defaultRegistry = otherGameType.CreateNonExistingRegistry(ServiceProvider);

        IGameRegistry eawRegistry;
        IGameRegistry focRegistry;

        if (gameType == GameType.Eaw)
        {
            eawRegistry = gameRegistry;
            focRegistry = defaultRegistry;
        }
        else
        {
            eawRegistry = defaultRegistry;
            focRegistry = gameRegistry;
        }

        return new GameRegistryContainer(eawRegistry, focRegistry);
    }

    protected override void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<GameRegistryContainer> info)
    {
        if (!shallInitSuccessfully)
            return;
        var registrySetupData = TestGameRegistrySetupData.Installed(info.GameType, info.GameDirectory!);
        registrySetupData.Create(ServiceProvider);
    }

    [Fact]
    public void TestInvalidArgs_Throws()
    {
        var eawRegistry = GameType.Eaw.CreateNonExistingRegistry(ServiceProvider);
        var focRegistry = GameType.Foc.CreateNonExistingRegistry(ServiceProvider);
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(null!, focRegistry, false, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(eawRegistry, null!, false, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(eawRegistry, focRegistry, false, null!));
        Assert.Throws<ArgumentException>(() => new RegistryGameDetector(focRegistry, focRegistry, false, ServiceProvider));
        Assert.Throws<ArgumentException>(() => new RegistryGameDetector(eawRegistry, eawRegistry, false, ServiceProvider));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Dispose_ShallDisposeRegistries(GameIdentity identity)
    {
        var info = SetupGame(identity);
        var detector = new RegistryGameDetector(info.DetectorSetupInfo!.EawRegistry!, info.DetectorSetupInfo!.FocRegistry!,
            false, ServiceProvider);
        detector.Dispose();

        Assert.Throws<ObjectDisposedException>(() => info.DetectorSetupInfo.EawRegistry!.CdKey);
        Assert.Throws<ObjectDisposedException>(() => info.DetectorSetupInfo.FocRegistry!.CdKey);
    }
}