using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class DirectoryGameDetectorTest
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;

    public DirectoryGameDetectorTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
    }

    private DirectoryGameDetector CreateDetector(string path)
    { 
        var directory = _fileSystem.DirectoryInfo.New(path);
        return new DirectoryGameDetector(directory, _serviceProvider);
    }

    [Fact]
    public void InvalidArgs_Throws()
    {
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null!, null!));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(_fileSystem.DirectoryInfo.New("Game"), null!));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null!, sp.Object));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindGameLocation_GamesNotInstalled_DirectoryNotFound(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);
        var detector = CreateDetector("Game");
       
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindGameLocation_GamesNotInstalled_GameExeNotFound(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);

        _fileSystem.Initialize().WithSubdirectory("Game");
        var detector = CreateDetector("Game");

        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);
    }

    [Theory]
    [InlineData(GameType.Eaw, PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName)]
    [InlineData(GameType.Foc, PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName)]
    public void FindGameLocation_GamesNotInstalled_GameDataWithMegaFilesNotFound(GameType gameType, string exeName)
    {
        var options = new GameDetectorOptions(gameType);
        _fileSystem.Initialize().WithSubdirectory("Game").WithFile($"Game/{exeName}");
        var detector = CreateDetector("Game");
        
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);

        _fileSystem.Initialize().WithSubdirectory("Game/Data");

        result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindGameLocation_GamesInstalled(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);
        var game  = _fileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.Disk), _serviceProvider);
        var detector = CreateDetector(game.Directory.FullName);

        var result = detector.FindGameLocation(options);
        Assert.Equal(game.Directory.FullName, result.Location!.FullName);
        Assert.True(result.IsInstalled);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindGameLocation_GamesInstalled_WrongPathGiven(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);
        _fileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.Disk), _serviceProvider);
        var detector = CreateDetector("this_path_does_not_contain_a_game");

        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);
    }

    [Theory]
    [InlineData(GameType.Eaw, GameType.Foc)]
    [InlineData(GameType.Foc, GameType.Eaw)]
    public void FindGameLocation_GamesInstalled_WrongTypeGiven(GameType gameType, GameType searchType)
    {
        var options = new GameDetectorOptions(searchType);
        _fileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.Disk), _serviceProvider);
        var detector = CreateDetector("this_path_does_not_contain_a_game");

        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
        Assert.False(result.IsInstalled);
    }


    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_Integration_GameInstalled(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);
        var game = _fileSystem.InstallGame(new GameIdentity(gameType, GamePlatform.Disk), _serviceProvider);
        var detector = CreateDetector(game.Directory.FullName);

        var result = detector.Detect(options);
        Assert.Equal(game.Directory.FullName, result.GameLocation!.FullName);

        Assert.True(detector.TryDetect(options, out result));
        Assert.Equal(game.Directory.FullName, result.GameLocation!.FullName);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Detect_Integration_GameNotInstalled(GameType gameType)
    {
        var options = new GameDetectorOptions(gameType);
        _fileSystem.Initialize().WithSubdirectory("game/bin");
        var detector = CreateDetector("game/bin");

        var result = detector.Detect(options);
        Assert.Null(result.GameLocation);

        Assert.False(detector.TryDetect(options, out result));
        Assert.Null(result.GameLocation);
    }
}