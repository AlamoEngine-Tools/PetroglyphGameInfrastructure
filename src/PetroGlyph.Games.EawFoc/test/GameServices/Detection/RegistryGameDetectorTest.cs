using System;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;
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
        return SetupRegistry(game, TestGameRegistrySetupData.Installed(game.Type, game.Directory));
    }

    protected override GameDetectorTestInfo<GameRegistryContainer> SetupForRequiredInitialization(GameIdentity identity)
    {
        var game = InstallGame(identity);
        return SetupRegistry(game, TestGameRegistrySetupData.Uninitialized(identity.Type));
    }

    private GameDetectorTestInfo<GameRegistryContainer> SetupRegistry(IGame game, TestGameRegistrySetupData registrySetup)
    {
        var otherGameType = game.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw;

        var gameRegistry = registrySetup.Create(ServiceProvider);
        var defaultRegistry = otherGameType.CreateNonExistingRegistry(ServiceProvider);

        IGameRegistry eawRegistry;
        IGameRegistry focRegistry;

        if (game.Type == GameType.Eaw)
        {
            eawRegistry = gameRegistry;
            focRegistry = defaultRegistry;
        }
        else
        {
            eawRegistry = defaultRegistry;
            focRegistry = gameRegistry;
        }

        return new(game.Type, game.Directory, new GameRegistryContainer(eawRegistry, focRegistry));
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
}