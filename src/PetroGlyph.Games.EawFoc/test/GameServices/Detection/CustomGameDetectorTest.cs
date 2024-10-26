using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class CustomGameDetectorTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MockFileSystem _fileSystem = new();

    public CustomGameDetectorTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_TryDetect_FindGameLocationThrowsException(GameType gameType)
    {
        var expected = GameDetectionResult.NotInstalled(gameType);
        foreach (var platform in GITestUtilities.EnumerateRealPlatforms())
        {
            var gameInfo = _fileSystem.InstallGame(new GameIdentity(gameType, platform), _serviceProvider);
            Assert.NotNull(gameInfo);

            var detector = new CallbackGameDetectorBase(FindGameThrowsException, _serviceProvider, false);

            Assert.Throws<Exception>(() => detector.Detect(gameType, []));

            Assert.False(detector.TryDetect(gameType, [], out var result));
            expected.AssertEqual(result);
        }
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_TryDetect_FindGameLocationReturnsGameWhenItDoesNotExistsOnDisk(GameType gameType)
    {
        var expected = GameDetectionResult.NotInstalled(gameType);
        var detector = new CallbackGameDetectorBase(ReturnsGame, _serviceProvider, false);

        var result = detector.Detect(gameType, []);
        expected.AssertEqual(result);

        Assert.False(detector.TryDetect(gameType, [], out result));
        expected.AssertEqual(result);
    }

    [Theory]
    [InlineData(GameType.Eaw, true)]
    [InlineData(GameType.Eaw, false)]
    [InlineData(GameType.Foc, true)]
    [InlineData(GameType.Foc, false)]
    public void Detect_TryDetect_InitializationRequestedIsTriggeredWhenSupported_DoNotHandle(GameType gameType, bool supportInitialization)
    {
        var requiredInit = GameDetectionResult.RequiresInitialization(gameType);
        foreach (var platform in GITestUtilities.EnumerateRealPlatforms())
        {
            var gameInfo = _fileSystem.InstallGame(new GameIdentity(gameType, platform), _serviceProvider);
            Assert.NotNull(gameInfo);

            var state = new DetectorState();
            var detector = new CallbackWithStateGameDetectorBase(state, HandleRequiredInitialization, _serviceProvider, supportInitialization);

            var eventRaised = false;

            detector.InitializationRequested += (_, e) =>
            {
                state.Value = "some Value";
                eventRaised = true;
                // Do not handle
                e.Handled = false;
            };

            var result = detector.Detect(gameType, [platform]);
            requiredInit.AssertEqual(result);
            Assert.Equal(supportInitialization, eventRaised);

            // Reset
            eventRaised = false;
            state.Value = null;

            Assert.False(detector.TryDetect(gameType, [platform], out result));
            requiredInit.AssertEqual(result);
            Assert.Equal(supportInitialization, eventRaised);
        }
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_TryDetect_InitializationRequestedIsTriggeredAndHandled(GameType gameType)
    {
        foreach (var platform in GITestUtilities.EnumerateRealPlatforms())
        { 
            var gameInfo = _fileSystem.InstallGame(new GameIdentity(gameType, platform), _serviceProvider);
            Assert.NotNull(gameInfo);

            var installed = GameDetectionResult.FromInstalled(new GameIdentity(gameType, platform), gameInfo.Directory);

            var state = new DetectorState();
            var detector = new CallbackWithStateGameDetectorBase(state, HandleRequiredInitialization, _serviceProvider, true);

            var eventRaised = false;

            detector.InitializationRequested += (_, e) =>
            {
                state.Value = gameInfo.Directory.FullName;
                e.Handled = true;
                eventRaised = true;
            };
            var result = detector.Detect(gameType, [platform]);
            installed.AssertEqual(result);
            Assert.True(eventRaised);

            // Reset
            eventRaised = false;
            state.Value = null;

            Assert.True(detector.TryDetect(gameType, [platform], out result));
            installed.AssertEqual(result);
            Assert.True(eventRaised);
        }
    }

    private GameDetectorBase.GameLocationData HandleRequiredInitialization(GameType type, DetectorState state)
    {
        return state.Value is null
            ? GameDetectorBase.GameLocationData.RequiresInitialization
            : new GameDetectorBase.GameLocationData(_fileSystem.DirectoryInfo.New(state.Value.ToString()!));
    }

    private static GameDetectorBase.GameLocationData FindGameThrowsException(GameType arg)
    {
        throw new Exception("Test exception");
    }

    private GameDetectorBase.GameLocationData ReturnsGame(GameType arg)
    {
        return new GameDetectorBase.GameLocationData(_fileSystem.DirectoryInfo.New($"games/{arg}"));
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