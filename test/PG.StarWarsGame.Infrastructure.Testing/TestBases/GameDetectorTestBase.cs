using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract partial class GameDetectorTestBase<T> : CommonTestBase
{
    protected abstract bool SupportInitialization { get; }

    protected abstract ICollection<GamePlatform> SupportedPlatforms { get; }

    protected abstract bool CanDisableInitRequest { get; }

    protected abstract IGameDetector CreateDetector(GameDetectorTestInfo<T> gameInfo, bool shallHandleInitialization);

    protected abstract GameDetectorTestInfo<T> SetupGame(GameIdentity gameIdentity);

    protected abstract GameDetectorTestInfo<T> SetupForRequiredInitialization(GameIdentity gameIdentity);

    protected abstract void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<T> info);

    protected void TestDetectorCore(
        GameIdentity identity,
        Func<GameIdentity, GameDetectorTestInfo<T>>? customSetup,
        Func<GameDetectorTestInfo<T>, GameDetectionResult> expectedResultFactory,
        params GamePlatform[] queryPlatforms)
    {
        TestDetectorCore(identity, false, customSetup, expectedResultFactory, null, queryPlatforms);
    }

    protected void TestDetectorCore(
        GameIdentity identity,
        bool shallHandleInitialization,
        Func<GameIdentity, GameDetectorTestInfo<T>>? customSetup,
        Func<GameDetectorTestInfo<T>, GameDetectionResult> expectedResultFactory,
        Predicate<object>? handleInitialization,
        params GamePlatform[] queryPlatforms)
    {
        var gameInfo = customSetup is null
            ? SetupGame(identity)
            : customSetup(identity);

        var expectedResult = expectedResultFactory(gameInfo);

        if (!SupportInitialization)
            shallHandleInitialization = false;

        var detector = CreateDetector(gameInfo, shallHandleInitialization);

        var shouldTriggerInitEvent = SupportInitialization && SupportedPlatforms.Contains(identity.Platform) && shallHandleInitialization;
        var eventTriggered = false;

        detector.InitializationRequested += (s, e) =>
        {
            Assert.True(SupportInitialization);
            eventTriggered = true;
            if (handleInitialization is not null)
                e.Handled = handleInitialization(e);
        };

        var result = detector.Detect(identity.Type, queryPlatforms);
        expectedResult.AssertEqual(result);
        Assert.Equal(shouldTriggerInitEvent, eventTriggered);

        // Reset state for TryDetect
        eventTriggered = false;
        if (shallHandleInitialization) 
            SetupForRequiredInitialization(identity);

        Assert.Equal(expectedResult.Installed, detector.TryDetect(identity.Type, queryPlatforms, out result));
        expectedResult.AssertEqual(result);
        Assert.Equal(shouldTriggerInitEvent, eventTriggered);
    }

    protected class GameDetectorTestInfo<TInfo>(GameType gameType, IDirectoryInfo? directoryInfo, TInfo? setupInfo)
    {
        public GameType GameType { get; } = gameType;

        public IDirectoryInfo? GameDirectory { get; } = directoryInfo;

        public TInfo? DetectorSetupInfo { get; } = setupInfo;
    }
}