using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GamePlatformIdentifierTest : GameInfrastructureTestBase
{
    private readonly GamePlatformIdentifier _platformIdentifier;

    public GamePlatformIdentifierTest()
    {
        _platformIdentifier = new GamePlatformIdentifier(ServiceProvider);
    }

    [Fact]
    public void NullArgs_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GamePlatformIdentifier(null!));

        IDirectoryInfo? nullRef = null;
        Assert.Throws<ArgumentNullException>(() => _platformIdentifier.GetGamePlatform(default, ref nullRef!));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void GetGamePlatform_WrongGameInstalledReturnsUndefined(GameIdentity identity)
    {
        var oppositeGameType = identity.Type == GameType.Foc ? GameType.Eaw : GameType.Foc;
        var game = GetOrÍnstallGame(new GameIdentity(oppositeGameType, identity.Platform));
        var gameLocation = game.Directory;

        var actual = _platformIdentifier.GetGamePlatform(identity.Type, ref gameLocation);
        Assert.True(GamePlatform.Undefined == actual, $"Expected value to be Undefined for platform {identity.Platform}");
        Assert.Equal(game.Directory.FullName, gameLocation.FullName);
    }


    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void GetGamePlatform_NoGameInstalledReturnsUndefined(GameType queryGameType)
    {
        var gameLocation = FileSystem.DirectoryInfo.New("noGameDir");
        var locRef = gameLocation;
        var actual = _platformIdentifier.GetGamePlatform(queryGameType, ref locRef);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(gameLocation.FullName, locRef.FullName);
    }
    
    [Fact]
    public void GetGamePlatform_FocOriginWithSanitization()
    {
        GetOrÍnstallGame(new GameIdentity(GameType.Foc, GamePlatform.Origin));

        var locRef = GameInstallation.GetWrongOriginFocRegistryLocation();
        var locStore = locRef;

        var actual = _platformIdentifier.GetGamePlatform(GameType.Foc, ref locRef);
        Assert.Equal(GamePlatform.Origin, actual);
        Assert.NotEqual(locStore, locRef);
    }

    [Theory]
    [MemberData(nameof(FocGamePaths))]
    public void GetGamePlatform_CannotGetPlatform_GameExeNotFound_Foc(string gamePath)
    {
        GetGamePlatform_CannotGetPlatform_GameExeNotFound(GameType.Foc, gamePath);
    }
    
    [Theory]
    [MemberData(nameof(EawGamePaths))]
    public void GetGamePlatform_CannotGetPlatform_GameExeNotFound_Eaw(string subPath)
    {
        GetGamePlatform_CannotGetPlatform_GameExeNotFound(GameType.Eaw, subPath);
    }

    private void GetGamePlatform_CannotGetPlatform_GameExeNotFound(GameType gameType, string gamePath)
    {
        FileSystem.Initialize().WithFile(FileSystem.Path.Combine(gamePath, "Data", "megafiles.xml"));

        var loc = FileSystem.DirectoryInfo.New(gamePath);

        var actual = _platformIdentifier.GetGamePlatform(gameType, ref loc);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(loc, loc);
    }

    [Theory]
    [MemberData(nameof(FocGamePaths))]
    public void GetGamePlatform_CannotGetPlatform_DataAndMegafilesXmlNotFound_Foc(string gamePath)
    {
        GetGamePlatform_CannotGetPlatform_DataAndMegafilesXmlNotFound(GameType.Foc, gamePath);
    }

    [Theory]
    [MemberData(nameof(EawGamePaths))]
    public void GetGamePlatform_CannotGetPlatform_DataAndMegafilesXmlNotFound_Eaw(string gamePath)
    {
        GetGamePlatform_CannotGetPlatform_DataAndMegafilesXmlNotFound(GameType.Eaw, gamePath);
    }
    
    private void GetGamePlatform_CannotGetPlatform_DataAndMegafilesXmlNotFound(GameType gameType, string gamePath)
    {
        FileSystem.Initialize().WithFile(FileSystem.Path.Combine(gamePath, PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName));
        var loc = FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(gamePath));

        var actual = _platformIdentifier.GetGamePlatform(gameType, ref loc);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(loc, loc);

        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(gamePath, "Data"));
        actual = _platformIdentifier.GetGamePlatform(gameType, ref loc);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(loc, loc);
    }

    public static IEnumerable<object[]> EawGamePaths()
    {
        yield return ["/"];
        yield return ["GameData"];
        yield return ["eaw/GameData"];
        yield return ["eaw"];
        yield return ["steam/apps/12345/GameData"];
    }

    public static IEnumerable<object[]> FocGamePaths()
    {
        yield return ["/"];
        yield return ["EAWX"];
        yield return ["games/EAWX"];
        yield return ["corruption"];
        yield return ["games/corruption"];
        yield return ["steam/apps/12345/corruption"];
        yield return ["foc"];
    }
}