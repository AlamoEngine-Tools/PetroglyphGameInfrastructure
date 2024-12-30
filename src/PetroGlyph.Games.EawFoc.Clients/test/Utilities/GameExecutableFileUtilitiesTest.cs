using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Utilities;

public class GameExecutableFileUtilitiesTest : CommonTestBase
{
    public static IEnumerable<object[]> GetGameExeNamesTestData()
    {
        var gameTypes = new[] { GameType.Eaw, GameType.Foc };
        foreach (var platform in GITestUtilities.RealPlatforms)
        {
            foreach (var gameType in gameTypes)
            {
                var id = new GameIdentity(gameType, platform);

                if (platform == GamePlatform.SteamGold)
                {
                    yield return [id, GameBuildType.Debug, "StarWarsI.exe"];
                    yield return [id, GameBuildType.Release, "StarWarsG.exe"];
                }
                else
                {
                    if (gameType == GameType.Eaw)
                        yield return [id, GameBuildType.Release, "sweaw.exe"];
                    else
                        yield return [id, GameBuildType.Release, "swfoc.exe"];
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void GetExecutableForGame_GameExeFilesNotInstalled_ReturnsNull(GameIdentity gameIdentity)
    {
        var gameDir = FileSystem.DirectoryInfo.New("Game");
        gameDir.Create();

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, "MyGame", ServiceProvider);

        var buildTypes = new List<GameBuildType> { GameBuildType.Release };
        if (game.Platform is GamePlatform.SteamGold)
            buildTypes.Add(GameBuildType.Debug);

        foreach (var buildType in buildTypes)
        {
            var exeFile = GameExecutableFileUtilities.GetExecutableForGame(game, buildType);
            Assert.Null(exeFile);
        }
    }

    [Theory]
    [MemberData(nameof(GetGameExeNamesTestData))]
    public void GetExecutableForGame_ReturnsFileHandle(GameIdentity gameIdentity, GameBuildType buildType, string expectedName)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        if (buildType == GameBuildType.Debug)
            game.InstallDebug();

        var exeFile = GameExecutableFileUtilities.GetExecutableForGame(game, buildType);
        Assert.NotNull(exeFile);
        Assert.Equal(expectedName, exeFile.Name);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void GetExecutableForGame_SteamHasReleaseButNotDebugFiles(GameType gameType)
    {
        var game = FileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.SteamGold), ServiceProvider);

        var releaseExe = GameExecutableFileUtilities.GetExecutableForGame(game, GameBuildType.Release);
        Assert.NotNull(releaseExe);

        var debugExe = GameExecutableFileUtilities.GetExecutableForGame(game, GameBuildType.Debug);
        Assert.Null(debugExe);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void GetExecutableForGame_DebugFilesForOtherThanSteam_ShouldReturnNull(GameIdentity identity)
    {
        if (identity.Platform == GamePlatform.SteamGold)
            return;

        var game = FileSystem.InstallGame(identity, ServiceProvider);
        using var _ = FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "StarWarsG.exe"));

        Assert.Null(GameExecutableFileUtilities.GetExecutableForGame(game, GameBuildType.Debug));
    }
}