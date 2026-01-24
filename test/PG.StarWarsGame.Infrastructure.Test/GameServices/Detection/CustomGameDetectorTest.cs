using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using System;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class CustomGameDetectorTest : GameInfrastructureTestBase
{
    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Detect_TryDetect_FindGameLocationThrowsException(GameIdentity identity)
    {
        var expected = GameDetectionResult.NotInstalled(identity.Type);
        var game = GetOrCreateGameInstallation(identity);
        Assert.NotNull(game);

        var detector = new CallbackGameDetectorBase(FindGameThrowsException, ServiceProvider, false);

        Assert.Throws<Exception>(() => detector.Detect(identity.Type));

        Assert.False(detector.TryDetect(identity.Type, [], out var result));
        expected.AssertEqual(result);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_TryDetect_FindGameLocationReturnsGameWhenItDoesNotExistsOnDisk(GameType gameType)
    {
        var expected = GameDetectionResult.NotInstalled(gameType);
        var detector = new CallbackGameDetectorBase(ReturnsGame, ServiceProvider, false);

        var result = detector.Detect(gameType);
        expected.AssertEqual(result);

        Assert.False(detector.TryDetect(gameType, [], out result));
        expected.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Detect_TryDetect_InitializationRequestedIsNotTriggeredWhenSupported_DoNotHandle(GameIdentity identity)
    {
        var game = GetOrCreateGameInstallation(identity).Game;
        Assert.NotNull(game);
        Detect_TryDetect_InitializationRequestedIsTriggeredWhenSupported_DoNotHandle(game, false);
        Detect_TryDetect_InitializationRequestedIsTriggeredWhenSupported_DoNotHandle(game, true);
    }
    
    private void Detect_TryDetect_InitializationRequestedIsTriggeredWhenSupported_DoNotHandle(IGame game, bool supportInitialization)
    {
        var requiredInit = GameDetectionResult.RequiresInitialization(game.Type);

        var state = new DetectorState();
        var detector = new CallbackWithStateGameDetectorBase(state, HandleRequiredInitialization, ServiceProvider, supportInitialization);

        var eventRaised = false;

        detector.InitializationRequested += (_, e) =>
        {
            state.Value = "some Value";
            eventRaised = true;
            // Do not handle
            e.Handled = false;
        };

        var result = detector.Detect(game.Type, game.Platform);
        requiredInit.AssertEqual(result);
        Assert.Equal(supportInitialization, eventRaised);

        // Reset
        eventRaised = false;
        state.Value = null;

        Assert.False(detector.TryDetect(game.Type, [game.Platform], out result));
        requiredInit.AssertEqual(result);
        Assert.Equal(supportInitialization, eventRaised);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void Detect_TryDetect_InitializationRequestedIsTriggeredAndHandled(GameIdentity identity)
    {
        var game = GetOrCreateGameInstallation(identity).Game;
        Assert.NotNull(game);

        var installed = GameDetectionResult.FromInstalled(identity, game.Directory);

        var state = new DetectorState();
        var detector = new CallbackWithStateGameDetectorBase(state, HandleRequiredInitialization, ServiceProvider, true);

        var eventRaised = false;

        detector.InitializationRequested += (_, e) =>
        {
            state.Value = game.Directory.FullName;
            e.Handled = true;
            eventRaised = true;
        };
        var result = detector.Detect(identity.Type, identity.Platform);
        installed.AssertEqual(result);
        Assert.True(eventRaised);

        // Reset
        eventRaised = false;
        state.Value = null;

        Assert.True(detector.TryDetect(identity.Type, [identity.Platform], out result));
        installed.AssertEqual(result);
        Assert.True(eventRaised);
    }

    private GameDetectorBase.GameLocationData HandleRequiredInitialization(GameType type, DetectorState state)
    {
        return state.Value is null
            ? GameDetectorBase.GameLocationData.RequiresInitialization
            : new GameDetectorBase.GameLocationData(FileSystem.DirectoryInfo.New(state.Value.ToString()!));
    }

    private static GameDetectorBase.GameLocationData FindGameThrowsException(GameType arg)
    {
        throw new Exception("Test exception");
    }

    private GameDetectorBase.GameLocationData ReturnsGame(GameType arg)
    {
        return new GameDetectorBase.GameLocationData(FileSystem.DirectoryInfo.New($"games/{arg}"));
    }

    public class CallbackGameDetectorBase(
        Func<GameType, GameDetectorBase.GameLocationData> findGameLocationFunc,
        IServiceProvider serviceProvider,
        bool tryHandleInitialization) : GameDetectorBase(serviceProvider, tryHandleInitialization)
    {
        protected override GameLocationData FindGameLocation(GameType gameType)
        {
            return findGameLocationFunc(gameType);
        }
    }

    public class CallbackWithStateGameDetectorBase(
        DetectorState initState,
        Func<GameType, DetectorState, GameDetectorBase.GameLocationData> findGameLocationFunc,
        IServiceProvider serviceProvider,
        bool tryHandleInitialization) : GameDetectorBase(serviceProvider, tryHandleInitialization)
    {
        public DetectorState State { get; } = initState;

        protected override GameLocationData FindGameLocation(GameType gameType)
        {
            return findGameLocationFunc(gameType, State);
        }
    }

    public class DetectorState
    {
        public object? Value { get; set; }
    }
}