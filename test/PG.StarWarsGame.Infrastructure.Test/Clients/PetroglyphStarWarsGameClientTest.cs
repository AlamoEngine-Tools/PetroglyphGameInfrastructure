using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Clients;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients;

public class PetroglyphStarWarsGameClientTest : GameInfrastructureTestBase, IDisposable
{
    private readonly IGameClientFactory _clientFactory;
    private readonly TestGameProcessLauncher _processLauncher = new();

    private ITestingGameInstallation? _gameInstallation;

    protected virtual ICollection<GamePlatform> SupportedPlatforms => GITestUtilities.RealPlatforms;

    public virtual void Dispose()
    {
        _processLauncher.Dispose();
    }

    protected virtual void BeforePlay()
    {
    }

    protected override ITestingGameInstallation GetOrCreateGameInstallation(IGameIdentity? identity = null)
    {
        if (_gameInstallation is not null)
            return _gameInstallation;

        if (identity is not null)
        {
            if (!SupportedPlatforms.Contains(identity.Platform))
                throw new NotSupportedException($"The requested game platform '{identity.Platform}' is not supported by this client test");
            return _gameInstallation = GameInfrastructureTesting.Game(identity, ServiceProvider);
        }

        var newIdentity = new GameIdentity(Random.Enum<GameType>(), Random.Item(SupportedPlatforms));
        return _gameInstallation = GameInfrastructureTesting.Game(newIdentity, ServiceProvider);
    }

    public PetroglyphStarWarsGameClientTest()
    {
        _clientFactory = ServiceProvider.GetRequiredService<IGameClientFactory>();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
        TestGameProcessLauncher.RegisterAsService(serviceCollection, _processLauncher);
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGameClient(null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGameClient(GetOrCreateGameInstallation().Game, null!));
    }

    [Fact]
    public void Ctor_SetsGame()
    {
        var game = GetOrCreateGameInstallation().Game;
        using var client = new PetroglyphStarWarsGameClient(game, ServiceProvider);
        Assert.Same(game, client.Game);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void IsDebugAvailable_NoDebugFilesAvailable(GameIdentity gameIdentity)
    {
        using var client = _clientFactory.CreateClient(GetOrCreateGameInstallation(gameIdentity).Game);
        Assert.False(client.IsDebugAvailable());
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void IsDebugAvailable_DebugFilesAvailable(GameType gameType)
    {
        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, GamePlatform.SteamGold));
        gameInstallation.InstallDebug();
        using var client = _clientFactory.CreateClient(gameInstallation.Game);
        Assert.True(client.IsDebugAvailable());
    }

    [Fact]
    public void PlayDebug_NullArgs_Throws()
    {
        var game = GetOrCreateGameInstallation().Game;
        var client = _clientFactory.CreateClient(game);

        Assert.Throws<ArgumentNullException>(() => client.Play((IPhysicalMod)null!));
        Assert.Throws<ArgumentNullException>(() => client.Play((ArgumentCollection)null!));
        Assert.Throws<ArgumentNullException>(() => client.Debug(null!, true));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Play_CancelGameStart_Throws(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;
       
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        if (gameIdentity.Platform == GamePlatform.SteamGold)
            gameInstallation.InstallDebug();
        var mod = gameInstallation.InstallMod("MyMod", false).Mod;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);

        TestPlay(game, expectedProcessInfo, client => client.Play(ArgumentCollection.Empty), Cancel, true);
        TestPlay(game, expectedProcessInfo, client => client.Play(), Cancel, true);

        var expectedModProcessInfo = new GameProcessInfo(game, GameBuildType.Release,
            new ArgumentCollection([new ModArgumentList([new ModArgument(mod.Directory, game.Directory, false)])]));
        TestPlay(game, expectedModProcessInfo, client => client.Play(mod), Cancel, true);

        var expectedDebugProcessInfo = new GameProcessInfo(game,
            gameIdentity.Platform == GamePlatform.SteamGold ? GameBuildType.Debug : GameBuildType.Release,
            ArgumentCollection.Empty);
        TestPlay(game, expectedDebugProcessInfo, client => client.Debug(ArgumentCollection.Empty, true), Cancel, true);

        return;

        void Cancel(GameStartingEventArgs args)
        {
            args.Cancel = true;
        }
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void PlayDebug_ProcessLauncherThrows(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        if (gameIdentity.Platform == GamePlatform.SteamGold)
            gameInstallation.InstallDebug();
        var mod = gameInstallation.InstallMod("MyMod", false).Mod;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        
        TestPlay(game, expectedProcessInfo, client => client.Play(ArgumentCollection.Empty), shallThrowGameStartException: true, processLauncherShallThrowGameStartException:true);
        TestPlay(game, expectedProcessInfo, client => client.Play(), shallThrowGameStartException: true, processLauncherShallThrowGameStartException: true);

        var expectedModProcessInfo = new GameProcessInfo(game, GameBuildType.Release,
            new ArgumentCollection([new ModArgumentList([new ModArgument(mod.Directory, game.Directory, false)])]));
        TestPlay(game, expectedModProcessInfo, client => client.Play(mod), shallThrowGameStartException: true, processLauncherShallThrowGameStartException: true);

        var expectedDebugProcessInfo = new GameProcessInfo(game,
            gameIdentity.Platform == GamePlatform.SteamGold ? GameBuildType.Debug : GameBuildType.Release,
            ArgumentCollection.Empty);
        TestPlay(game, expectedDebugProcessInfo, client => client.Debug(ArgumentCollection.Empty, true), shallThrowGameStartException: true, processLauncherShallThrowGameStartException: true);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Play(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(game, expectedProcessInfo, client => client.Play());
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Play_Args(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var arg = new WindowedArgument();

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, new ArgumentCollection([arg]));
        TestPlay(game, expectedProcessInfo, client => client.Play(new ArgumentCollection([arg])));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Play_Mod(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        var mod = gameInstallation.InstallMod("MyMod").Mod;

        var expectedArguments = new ArgumentCollection([new ModArgumentList([
            new ModArgument(mod.Directory, game.Directory, mod.Type == ModType.Workshops)
        ])]);
        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, expectedArguments);
        TestPlay(game, expectedProcessInfo, client => client.Play(mod));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Debug_DebugIsAvailable(GameType gameType)
    {
        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, GamePlatform.SteamGold));
        gameInstallation.InstallDebug();
        var game = gameInstallation.Game;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Debug, ArgumentCollection.Empty);
        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, false));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Debug_FallbackToRelease(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(
            game, 
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, true));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Debug_DoNotFallbackToRelease_Throws(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var expectedProcessInfo = new GameProcessInfo(game, GameBuildType.Release, ArgumentCollection.Empty);
        TestPlay(
            game,
            expectedProcessInfo,
            client => client.Debug(ArgumentCollection.Empty, false),
            shallThrowGameStartException: true);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void PlayDebug_GameExecutablesNotAvailable_Throws(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

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
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void PlayDebug_Derived_OnGameStarting_ThrowsCustom(GameIdentity gameIdentity)
    {
        if (!SupportedPlatforms.Contains(gameIdentity.Platform))
            return;

        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var derivedClient = new MyTestClient((_, type) =>
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
        Action<GameStartingEventArgs>? startingHandler = null, 
        bool shallThrowGameStartException = false,
        bool processLauncherShallThrowGameStartException = false)
    {
        var client = _clientFactory.CreateClient(game);

        _processLauncher.ExpectedExecutable = GameExecutableFileUtilities.GetExecutableForGame(game, expectedProcessInfo.BuildType)!;
        _processLauncher.ExpectedProcessInfo = expectedProcessInfo;
        _processLauncher.ThrowsGameStartException = processLauncherShallThrowGameStartException;

        var startingEvent = false;
        client.GameStarting += (_, args) =>
        {
            AssertProcessStartInfo(expectedProcessInfo, new GameProcessInfo(args.Game, args.BuildType, args.GameArguments));
            startingEvent = true;
            startingHandler?.Invoke(args);
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

    private static void AssertProcessStartInfo(GameProcessInfo expected, GameProcessInfo actual)
    {
        Assert.Equal(expected.Game, actual.Game);
        Assert.Equal(expected.BuildType, actual.BuildType);
        Assert.Equal(expected.Arguments, actual.Arguments);
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