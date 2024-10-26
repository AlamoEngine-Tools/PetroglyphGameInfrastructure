using System;
using System.Globalization;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class GameFactoryTest
{
    private readonly GameFactory _factory;
    private readonly Mock<IGameNameResolver> _nameResolver;
    private readonly MockFileSystem _fileSystem = new();

    public GameFactoryTest()
    {
        var sc = new ServiceCollection();
        _nameResolver = new Mock<IGameNameResolver>();
        sc.AddSingleton(_nameResolver.Object);
        sc.AddSingleton<IFileSystem>(_fileSystem);
        _factory = new GameFactory(sc.BuildServiceProvider());
    }

    [Fact]
    public void NullCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameFactory(null));
    }

    [Fact]
    public void NotInstalled_Throws()
    {
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var detectionResult = GameDetectionResult.NotInstalled(GameType.Eaw);
        Assert.Throws<GameException>(() => _factory.CreateGame(detectionResult, CultureInfo.CurrentCulture));
    }

    [Fact]
    public void NoLocation_Throws()
    { 
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var identity = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(identity, null, true, CultureInfo.CurrentCulture));
    }

    [Fact]
    public void GameNotExists_Throws()
    {
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var identity = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        Assert.Throws<GameException>(() => _factory.CreateGame(identity, _fileSystem.DirectoryInfo.New("Game"), true, CultureInfo.CurrentCulture));
    }

    [Fact]
    public void IgnoreGameNotExists()
    { 
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var identity = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        var game = _factory.CreateGame(identity, new MockFileSystem().DirectoryInfo.New("Game"), false, CultureInfo.CurrentCulture);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }

    [Fact]
    public void GameFromIdentity()
    { 
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var identity = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        _fileSystem.Initialize()
            .WithSubdirectory("GameData")
            .WithFile("GameData/sweaw.exe");
        var game = _factory.CreateGame(identity, _fileSystem.DirectoryInfo.New("GameData"), false, CultureInfo.CurrentCulture);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }

    [Fact]
    public void GameFromDetectionResult()
    {
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var identity = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        var detectionResult = GameDetectionResult.FromInstalled(identity, _fileSystem.DirectoryInfo.New("GameData"));
        _fileSystem.Initialize()
            .WithSubdirectory("GameData")
            .WithFile("GameData/sweaw.exe");
        var game = _factory.CreateGame(detectionResult, CultureInfo.CurrentCulture);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }
}