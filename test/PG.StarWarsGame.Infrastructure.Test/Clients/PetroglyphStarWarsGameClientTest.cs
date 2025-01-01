using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Clients;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients;

public class PetroglyphStarWarsGameClientTest : CommonTestBase, IDisposable
{
    private readonly IGameClientFactory _clientFactory;
    private readonly TestGameProcessLauncher _processLauncher = new();

    protected virtual ICollection<GamePlatform> SupportedPlatforms => GITestUtilities.RealPlatforms;

    protected virtual void BeforePlay()
    {
    }

    public PetroglyphStarWarsGameClientTest()
    {
        _clientFactory = ServiceProvider.GetRequiredService<IGameClientFactory>();
    }

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        sc.AddSingleton<IGameProcessLauncher>(_processLauncher);
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGameClient(null!, ServiceProvider));
        var game = CreateRandomGame();
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGameClient(game, null!));
    }

    [Fact]
    public void Ctor_SetsGame()
    {
        var game = CreateRandomGame();
        using var client = new PetroglyphStarWarsGameClient(game, ServiceProvider);
        Assert.Same(game, client.Game);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void IsDebugAvailable_NoDebugFilesAvailable(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        using var client = _clientFactory.CreateClient(game);
        Assert.False(client.IsDebugAvailable());
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void IsDebugAvailable_DebugFilesAvailable(GameType gameType)
    {
        var game = FileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.SteamGold), ServiceProvider);
        game.InstallDebug();
        using var client = _clientFactory.CreateClient(game);
        Assert.True(client.IsDebugAvailable());
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Play(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(game, expectedProcessInfo, client => client.Play());
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Debug_DebugIsAvailable(GameType gameType)
    {
        var game = FileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.SteamGold), ServiceProvider);
        game.InstallDebug();

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Debug, ArgumentCollection.Empty);
        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, false));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Debug_FallbackToRelease(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(
            game, 
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, true));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Debug_DoNotFallbackToRelease_Throws(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, false),
            shallThrowGameStartException: true);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void PlayDebug_GameExecutablesNotAvailable_Throws(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);

        var exe = GameExecutableFileUtilities.GetExecutableForGame(game, GameBuildType.Release)!;
        exe.Delete();

        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Play(),
            shallThrowGameStartException: true);
        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, true),
            shallThrowGameStartException: true);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void PlayDebug_Derived_OnGameStarting_ThrowsCustom(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var derivedClient = new MyTestClient((arguments, type) =>
        {
            Assert.Equal(GameBuildType.Release, type);
            throw new Exception("Message");
        }, game, ServiceProvider);


        var e = Assert.Throws<Exception>(derivedClient.Play);
        Assert.Equal("Message", e.Message);

        e = Assert.Throws<Exception>(() => derivedClient.Debug(ArgumentCollection.Empty, true));
        Assert.Equal("Message", e.Message);
    }

    protected void TestPlay(
        IGame game, 
        GameProcessInfo expectedProcessInfo, 
        Func<IGameClient, IGameProcess> startGameAction, 
        bool shallThrowGameStartException = false)
    {
        var client = _clientFactory.CreateClient(game);

        _processLauncher.ExpectedExecutable = GameExecutableFileUtilities.GetExecutableForGame(game, expectedProcessInfo.BuildType)!;
        _processLauncher.ExpectedProcessInfo = expectedProcessInfo;

        var startingEvent = false;
        client.GameStarting += (_, args) =>
        {
            AssertProcessStartInfo(expectedProcessInfo, new GameProcessInfo(args.Game, args.BuildType, args.GameArguments));
            startingEvent = true;
        };
        var startedEvent = false;

        IGameProcess? processFromEvent = null;
        client.GameStarted += (_, process) =>
        {
            processFromEvent = process;
            startedEvent = true;
        };

        if (shallThrowGameStartException)
        {
            var e = Assert.Throws<GameStartException>(() => startGameAction(client));
            Assert.Same(game, e.Game);
            Assert.False(startedEvent);
        }
        else
        {
            BeforePlay();
            var process = startGameAction(client);
            Assert.Same(process, processFromEvent);
            Assert.True(startingEvent);
            Assert.True(startedEvent);
        }
    }

    private void AssertProcessStartInfo(GameProcessInfo expected, GameProcessInfo actual)
    {
        Assert.Equal(expected.Game, actual.Game);
        Assert.Equal(expected.BuildType, actual.BuildType);
        Assert.Equal(expected.Arguments, actual.Arguments);
    }

    public virtual void Dispose()
    {
        _processLauncher.Dispose();
    }

    private delegate void OnGameStartingDelegate(ArgumentCollection arguments, GameBuildType type);

    private class MyTestClient(OnGameStartingDelegate action, IGame game, IServiceProvider serviceProvider)
        : PetroglyphStarWarsGameClient(game, serviceProvider)
    {
        protected override void OnGameStarting(ArgumentCollection arguments, GameBuildType type)
        {
            action(arguments, type);
            base.OnGameStarting(arguments, type);
        }
    }
}