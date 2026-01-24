// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

/// <summary>
/// Provides a base class for testing game detectors in the Petroglyph Star Wars Game infrastructure.
/// </summary>
/// <typeparam name="T">The type of the setup information used by the game detector during testing.</typeparam>
public abstract partial class GameDetectorTestBase<T> : GameInfrastructureTestBase
{
    /// <summary>
    /// Gets a value indicating whether the game detector is aware of uninitialized game installation.
    /// </summary>
    protected abstract bool SupportInitialization { get; }

    /// <summary>
    /// Gets the collection of game platforms that are supported by the game detector.
    /// </summary>
    protected abstract ICollection<GamePlatform> SupportedPlatforms { get; }

    /// <summary>
    /// Gets a value indicating whether the game detector, which support initialization, support suppressing the initialization requests.
    /// </summary>
    protected abstract bool CanDisableInitRequest { get; }

    /// <summary>
    /// Creates an instance of a game detector for testing purposes.
    /// </summary>
    /// <param name="gameInfo">
    /// The setup information required for the game detector, including game type, directory information, 
    /// and any additional setup-specific data.
    /// </param>
    /// <param name="shallHandleInitialization">
    /// A value indicating whether the detector should handle initialization events.
    /// </param>
    /// <returns>An instance of <see cref="IGameDetector"/> configured based on the specified setup information.</returns>
    protected abstract IGameDetector CreateDetector(GameDetectorTestInfo<T> gameInfo, bool shallHandleInitialization);

    /// <summary>
    /// Sets up the game environment for testing based on the specified game identity.
    /// </summary>
    /// <param name="gameIdentity">The identity of the game.</param>
    /// <returns>A <see cref="GameDetectorTestInfo{T}"/> instance containing the setup information for the specified game identity.</returns>
    protected abstract GameDetectorTestInfo<T> SetupGame(GameIdentity gameIdentity);

    /// <summary>
    /// Prepares the necessary setup so that the game detector shall detect an uninitialized game.
    /// </summary>
    /// <param name="gameIdentity">The <see cref="GameIdentity"/> representing the uninitialized game.</param>
    /// <returns>A <see cref="GameDetectorTestInfo{T}"/> instance containing the setup information required for the test.</returns>
    protected abstract GameDetectorTestInfo<T> SetupForRequiredInitialization(GameIdentity gameIdentity);

    /// <summary>
    /// Handles the initialization process for the game detector during testing.
    /// </summary>
    /// <param name="shallInitSuccessfully">A value indicating whether the initialization should be simulated as successful.</param>
    /// <param name="info">The <see cref="GameDetectorTestInfo{T}"/> containing the setup information required for initialization.</param>
    protected abstract void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<T> info);

    /// <summary>
    /// Tests the core functionality of the game detector by verifying detection results for a specified game identity.
    /// </summary>
    /// <param name="identity">The <see cref="GameIdentity"/> representing the game to be detected.</param>
    /// <param name="customSetup">
    /// A function to provide custom setup logic for the test, returning a <see cref="GameDetectorTestInfo{T}"/> instance.
    /// If <see langword="null"/>, <see cref="SetupGame"/> is used.
    /// </param>
    /// <param name="expectedResultFactory">
    /// A function to create the expected <see cref="GameDetectionResult"/> based on the provided test information.</param>
    /// <param name="queryPlatforms">The platforms to query during by the detector.</param>
    protected void TestDetectorCore(
        GameIdentity identity,
        Func<GameIdentity, GameDetectorTestInfo<T>>? customSetup,
        Func<GameDetectorTestInfo<T>, GameDetectionResult> expectedResultFactory,
        params GamePlatform[] queryPlatforms)
    {
        TestDetectorCore(identity, false, customSetup, expectedResultFactory, null, queryPlatforms);
    }

    /// <summary>
    /// Tests the core functionality of the game detector by verifying detection results for a specified game identity.
    /// </summary>
    /// <param name="identity">The <see cref="GameIdentity"/> representing the game to be detected.</param>
    /// <param name="shallHandleInitialization"> A boolean indicating whether initialization events should be handled during detection.</param>
    /// <param name="customSetup">
    /// A function to provide custom setup logic for the test, returning a <see cref="GameDetectorTestInfo{T}"/> instance.
    /// If <see langword="null"/>, <see cref="SetupGame"/> is used.
    /// </param>
    /// <param name="expectedResultFactory">
    /// A function to create the expected <see cref="GameDetectionResult"/> based on the provided test information.</param>
    /// <param name="handleInitialization">
    /// A predicate to handle initialization events, allowing custom logic to determine if the event is handled.
    /// Can be <see langword="null"/> if no initialization handling is requested.
    /// </param>
    /// <param name="queryPlatforms">The platforms to query during by the detector.</param>
    /// <remarks>
    /// This method validates the detection result and ensures that initialization events are triggered or not
    /// based on the provided parameters and the detector's capabilities.
    /// </remarks>
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

        detector.InitializationRequested += (_, e) =>
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

    /// <summary>
    /// Represents information required for setting up the test environment for the game detector.
    /// </summary>
    /// <typeparam name="TInfo">The type of the custom setup information.</typeparam>
    protected class GameDetectorTestInfo<TInfo>(GameType gameType, IDirectoryInfo? directoryInfo, TInfo? setupInfo)
    {
        /// <summary>
        /// Gets the type of the game being tested.
        /// </summary>
        public GameType GameType { get; } = gameType;

        /// <summary>
        /// Gets the directory information for the to be detected game.
        /// </summary>
        /// <remarks>
        /// This property provides access to the directory where the game files are located.
        /// It may return <see langword="null"/> to indicate the game shall not get installed.
        /// </remarks>
        public IDirectoryInfo? GameDirectory { get; } = directoryInfo;

        /// <summary>
        /// Gets the optional, custom setup information required for configuring the game detector.
        /// </summary>
        public TInfo? DetectorSetupInfo { get; } = setupInfo;
    }
}