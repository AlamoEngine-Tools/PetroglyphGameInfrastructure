using System;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices;

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
        Assert.Throws<GameException>(() => factory.CreateGame(identity, new MockFileSystem().DirectoryInfo.FromDirectoryName("Game")));
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
        var game = factory.CreateGame(identity, new MockFileSystem().DirectoryInfo.FromDirectoryName("Game"), false);
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
        fs.AddDirectory("GameData");
        fs.AddFile("GameData/sweaw.exe", new MockFileData(string.Empty));
        var game = factory.CreateGame(identity, fs.DirectoryInfo.FromDirectoryName("GameData"), false);
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
        var detectionResult = new GameDetectionResult(identity, fs.DirectoryInfo.FromDirectoryName("GameData"));
        fs.AddDirectory("GameData");
        fs.AddFile("GameData/sweaw.exe", new MockFileData(string.Empty));
        var game = factory.CreateGame(detectionResult);
        Assert.Equal(identity.Platform, game.Platform);
        Assert.Equal(identity.Type, game.Type);
    }
}