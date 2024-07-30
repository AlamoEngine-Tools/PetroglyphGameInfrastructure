using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class DirectoryGameDetectorTest
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null, null));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), null));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null, sp.Object));
    }

    [Fact]
    public void TestNoGame()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.Detect(options);
        Assert.Null(result.GameLocation);
    }

    [Fact]
    public void TestNoEawGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
    }

    [Fact]
    public void TestNoFocGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var options = new GameDetectorOptions(GameType.Foc);
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
    }

    [Fact]
    public void TestAnyEawGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }

    [Fact]
    public void TestAnyFocGame()
    { 
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var options = new GameDetectorOptions(GameType.Foc);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }

    [Fact]
    public void TestInKnownSubFocGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Dir/EAWX/swfoc.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Dir"), sp.Object);
        var options = new GameDetectorOptions(GameType.Foc);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }

    [Fact]
    public void TestNotInKnownSubFocGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Dir/EAWX/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Dir"), sp.Object);
        var options = new GameDetectorOptions(GameType.Foc);
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
    }

    [Fact]
    public void TestInUnknownSubFocGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Dir/SomeDir/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Dir"), sp.Object);
        var options = new GameDetectorOptions(GameType.Foc);
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
    }

    [Fact]
    public void TestInKnownSubEawGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Dir/GameData/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Dir"), sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }

    [Fact]
    public void TestInKnownSubSubEawGame()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Dir/Sub/GameData/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Dir"), sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }


    [Fact]
    public void TestAnyFocGame_Integration()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/sweaw.exe");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var pi = new Mock<IGamePlatformIdentifier>();
        var options = new GameDetectorOptions(GameType.Eaw);
        pi.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(GamePlatform.GoG);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(pi.Object);
        var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("Game"), sp.Object);
        var result = detector.Detect(options);
        Assert.NotNull(result.GameLocation);
        Assert.Equal(GamePlatform.GoG, result.GameIdentity.Platform);
    }
}