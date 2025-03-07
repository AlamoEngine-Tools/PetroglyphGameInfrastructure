﻿using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public partial class GameDetectorTestBase<T>
{
    private void TestDetectorGameInstalled(GameIdentity identity, params GamePlatform[] queryPlatforms)
    {
        TestDetectorCore(
            identity,
            null,
            info => SupportedPlatforms.Contains(identity.Platform)
                ? GameDetectionResult.FromInstalled(identity, info.GameDirectory!)
                : GameDetectionResult.NotInstalled(identity.Type),
            queryPlatforms);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GameInstalled_DetectWithSinglePlatform(GameIdentity identity)
    { 
        TestDetectorGameInstalled(identity, identity.Platform);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GameInstalled_NoQueryPlatformSearchesAll(GameIdentity identity)
    {
        TestDetectorGameInstalled(identity, queryPlatforms: []);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GameInstalled_UndefinedPlatformPlatformSearchesAll(GameIdentity identity)
    {
        TestDetectorGameInstalled(identity, queryPlatforms: [GamePlatform.Undefined]);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GameInstalled_QueryContainsUndefinedSearchesAll(GameIdentity identity)
    {
        TestDetectorGameInstalled(identity, queryPlatforms: [GamePlatform.Disk, GamePlatform.Undefined]);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GameNotInstalled(GameIdentity identity)
    {
        var expected = GameDetectionResult.NotInstalled(identity.Type);
        TestDetectorCore(
            identity,
            _ => new GameDetectorTestInfo<T>(identity.Type, null, default), // Do not set up any game 
            _ => expected, 
            identity.Platform);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_TypeOfDesiredPlatformNotFound(GameIdentity identity)
    {
        // Install the opposite of the desired game type.
        var typeToInstall = identity.Type == GameType.Foc ? GameType.Eaw : GameType.Foc;

        var expected = GameDetectionResult.NotInstalled(identity.Type);

        TestDetectorCore(
            identity,
            _ => SetupGame(new GameIdentity(typeToInstall, identity.Platform)), // Set up the opposite game
            _ => expected,
            identity.Platform
        );
    }

    [Theory]
    [InlineData(GameType.Eaw, GamePlatform.SteamGold)]
    [InlineData(GameType.Eaw, GamePlatform.SteamGold, GamePlatform.Disk)]
    [InlineData(GameType.Foc, GamePlatform.SteamGold)]
    [InlineData(GameType.Foc, GamePlatform.SteamGold, GamePlatform.Disk)]
    public void Detect_TryDetect_GoG_PlatformDoesNotMatch(GameType gameType, params GamePlatform[] platforms)
    {
        var expected = GameDetectionResult.NotInstalled(gameType);
        TestDetectorCore(
            new GameIdentity(gameType, GamePlatform.GoG),
            null,
            _ => expected,
            platforms
        );
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_TestRequiresGameInitialization_DetectorDoesNotHandle(GameIdentity identity)
    {
        if (!SupportInitialization)
            return;

        if (!CanDisableInitRequest)
            return;

        TestDetectorCore(
            identity,
            SetupForRequiredInitialization,
            _ => SupportedPlatforms.Contains(identity.Platform)
                ? GameDetectionResult.RequiresInitialization(identity.Type)
                : GameDetectionResult.NotInstalled(identity.Type),
            queryPlatforms: []);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_TestRequiresGameInitialization_DetectorCanHandle_ButDoesNotHandle(GameIdentity identity)
    {
        if (!SupportInitialization)
            return;

        TestDetectorCore(
            identity,
            true,
            SetupForRequiredInitialization,
            _ => SupportedPlatforms.Contains(identity.Platform)
                ? GameDetectionResult.RequiresInitialization(identity.Type)
                : GameDetectionResult.NotInstalled(identity.Type),
            _ => false,
            queryPlatforms: []);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_TestRequiresGameInitialization_DetectorCanHandle_DoesHandleAndGameIsInitialized(GameIdentity identity)
    {
        if (!SupportInitialization)
            return;

        GameDetectorTestInfo<T> gameInfo = null!;
        TestDetectorCore(
            identity,
            true,
            i =>
            {
                var result = SetupForRequiredInitialization(i);
                gameInfo = result;
                return result;
            },
            i => SupportedPlatforms.Contains(identity.Platform)
                ? GameDetectionResult.FromInstalled(identity, i.GameDirectory!)
                : GameDetectionResult.NotInstalled(identity.Type),
            _ =>
            {
                HandleInitialization(true, gameInfo);
                return true;
            },
            queryPlatforms: []);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_TestRequiresGameInitialization_DetectorCanHandle_DoesHandleButGameIsNotInitialized(GameIdentity identity)
    {
        if (!SupportInitialization)
            return;

        GameDetectorTestInfo<T> gameInfo = null!;
        TestDetectorCore(
            identity,
            true,
            i =>
            {
                var result = SetupForRequiredInitialization(i);
                gameInfo = result;
                return result;
            },
            _ => SupportedPlatforms.Contains(identity.Platform)
                ? GameDetectionResult.RequiresInitialization(identity.Type)
                : GameDetectionResult.NotInstalled(identity.Type),
            _ =>
            {
                HandleInitialization(false, gameInfo);
                return true;
            },
            queryPlatforms: []);
    }
}