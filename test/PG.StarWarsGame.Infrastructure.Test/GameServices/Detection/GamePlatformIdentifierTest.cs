using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GamePlatformIdentifierTest
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly GamePlatformIdentifier _platformIdentifier;
    private readonly IServiceProvider _serviceProvider;

    public GamePlatformIdentifierTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _platformIdentifier = new GamePlatformIdentifier(_serviceProvider);
    }

    [Fact]
    public void NullArgs_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GamePlatformIdentifier(null!));

        IDirectoryInfo? nullRef = null;
        Assert.Throws<ArgumentNullException>(() => _platformIdentifier.GetGamePlatform(default, ref nullRef!));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void GetGamePlatform_WrongGameInstalledReturnsUndefined(GameType queryGameType)
    {
        foreach (var platform in GITestUtilities.RealPlatforms)
        {
            var installType = queryGameType == GameType.Foc ? GameType.Eaw : GameType.Foc;
            var game = _fileSystem.InstallGame(new GameIdentity(installType, platform), _serviceProvider);
            var gameLocation = game.Directory;

            var actual = _platformIdentifier.GetGamePlatform(queryGameType, ref gameLocation);
            Assert.True(GamePlatform.Undefined == actual, $"Expected value to be Undefined for platform {platform}");
            Assert.Equal(game.Directory.FullName, gameLocation.FullName);
        }
    }


    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void GetGamePlatform_NoGameInstalledReturnsUndefined(GameType queryGameType)
    {
        var gameLocation = _fileSystem.DirectoryInfo.New("noGameDir");
        var locRef = gameLocation;
        var actual = _platformIdentifier.GetGamePlatform(queryGameType, ref locRef);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(gameLocation.FullName, locRef.FullName);
    }
    
    [Fact]
    public void GetGamePlatform_FocOriginWithSanitization()
    {
        _fileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.Origin), _serviceProvider);

        var locRef = _fileSystem.GetWrongOriginFocRegistryLocation();
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
        _fileSystem.Initialize().WithFile(_fileSystem.Path.Combine(gamePath, "Data", "megafiles.xml"));

        var loc = _fileSystem.DirectoryInfo.New(gamePath);

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
        _fileSystem.Initialize().WithFile(_fileSystem.Path.Combine(gamePath, PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName));
        var loc = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(gamePath));

        var actual = _platformIdentifier.GetGamePlatform(gameType, ref loc);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(loc, loc);

        _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(gamePath, "Data"));
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