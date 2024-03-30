using System;
using System.Globalization;
using System.IO.Abstractions;
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
    [Fact]
    public void NullCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameFactory(null));
        Assert.Throws<ArgumentNullException>(() => new GameFactory(null, null, null));
        var nameResolver = new Mock<IGameNameResolver>();
        Assert.Throws<ArgumentNullException>(() => new GameFactory(nameResolver.Object, null, null));
    }

    [Fact]
    public void FaultedDetection_Throws()
    {
        var sp = new Mock<IServiceProvider>();
        var factory = new GameFactory(sp.Object);
        Assert.Throws<GameException>(() =>
            factory.CreateGame(new GameDetectionResult(GameType.EaW, new Exception())));
    }

    [Fact]
    public void NotInstalled_Throws()
    {
        var sp = new Mock<IServiceProvider>();
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var detectionResult = GameDetectionResult.NotInstalled(GameType.EaW);
        Assert.Throws<GameException>(() =>
            factory.CreateGame(detectionResult));
    }

    [Fact]
    public void NoLocation_Throws()
    {
        var sp = new Mock<IServiceProvider>();
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var identity = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        Assert.Throws<ArgumentNullException>(() => factory.CreateGame(identity, null));
    }

    [Fact]
    public void GameNotExists_Throws()
    {
#if NET
        return;
#endif
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(new MockFileSystem());
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var identity = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        Assert.Throws<GameException>(() => factory.CreateGame(identity, new MockFileSystem().DirectoryInfo.New("Game")));
    }

    [Fact]
    public void IgnoreGameNotExists()
    {
        var sp = new Mock<IServiceProvider>();
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var identity = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        var game = factory.CreateGame(identity, new MockFileSystem().DirectoryInfo.New("Game"), false);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }

    [Fact]
    public void GameFromIdentity()
    {
        var sp = new Mock<IServiceProvider>();
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var identity = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        var fs = new MockFileSystem();
        fs.Initialize()
            .WithSubdirectory("GameData")
            .WithFile("GameData/sweaw.exe");
        var game = factory.CreateGame(identity, fs.DirectoryInfo.New("GameData"), false);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }

    [Fact]
    public void GameFromDetectionResult()
    {
        var sp = new Mock<IServiceProvider>();
        var nameResolver = new Mock<IGameNameResolver>();
        nameResolver.Setup(r => r.ResolveName(It.IsAny<IGameIdentity>(), It.IsAny<CultureInfo>())).Returns("GameName");
        var culture = CultureInfo.GetCultureInfo("de");
        var factory = new GameFactory(nameResolver.Object, culture, sp.Object);
        var identity = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        var fs = new MockFileSystem();
        var detectionResult = new GameDetectionResult(identity, fs.DirectoryInfo.New("GameData"));
        fs.Initialize()
            .WithSubdirectory("GameData")
            .WithFile("GameData/sweaw.exe");
        var game = factory.CreateGame(detectionResult);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }
}