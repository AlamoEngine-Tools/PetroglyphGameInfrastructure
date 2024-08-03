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

public class GameDetectorTest
{
    [Fact]
    public void TestFocGameExists()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, false);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.Setup(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { Location = fs.DirectoryInfo.New("Game") });


        var result = detector.Object.Detect(options);

        Assert.NotNull(result.GameLocation);
        Assert.Equal(platform, result.GameIdentity.Platform);
        Assert.Equal(type, result.GameIdentity.Type);
    }

    [Fact]
    public void TestFocGameNotExists()
    {
        const GamePlatform platform = GamePlatform.Disk;

        var fs = new MockFileSystem();

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, false);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.Setup(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { Location = fs.DirectoryInfo.New("Game") });


        var result = detector.Object.Detect(options);

        Assert.Null(result.GameLocation);
    }

    [Fact]
    public void TestExceptional()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, false);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.Setup(d => d.FindGameLocation(options)).Throws<Exception>();


        var result = detector.Object.Detect(options);

        Assert.NotNull(result.Error);
        Assert.Equal(type, result.GameIdentity.Type);
    }

    [Fact]
    public void TestNotRaisingEvent()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, false);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.Setup(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { InitializationRequired = true });

        var eventRaised = false;
        detector.Object.InitializationRequested += (_, _) => eventRaised = true;

        var result = detector.Object.Detect(options);

        Assert.False(eventRaised);
        Assert.Null(result.GameLocation);
        Assert.Equal(type, result.GameIdentity.Type);
    }

    [Fact]
    public void TestRaisingEventHandled()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, true);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.SetupSequence(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { InitializationRequired = true })
            .Returns(new GameDetectorBase.GameLocationData { Location = fs.DirectoryInfo.New("Game") });

        var eventRaised = false;
        detector.Object.InitializationRequested += (_, args) =>
        {
            args.Handled = true;
            eventRaised = true;
        };

        var result = detector.Object.Detect(options);

        Assert.True(eventRaised);
        Assert.NotNull(result.GameLocation);
        Assert.Equal(type, result.GameIdentity.Type);
    }

    [Fact]
    public void TestRaisingEventNotHandled()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, true);

        var options = new GameDetectorOptions(GameType.Foc);

        detector.Setup(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { InitializationRequired = true });

        var eventRaised = false;
        detector.Object.InitializationRequested += (_, _) => { eventRaised = true; };

        var result = detector.Object.Detect(options);

        Assert.True(eventRaised);
        Assert.Null(result.GameLocation);
        Assert.Equal(type, result.GameIdentity.Type);
    }

    [Fact]
    public void TestWrongPlatform()
    {
        const GamePlatform platform = GamePlatform.Disk;
        const GameType type = GameType.Foc;

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");

        var identifier = new Mock<IGamePlatformIdentifier>();
        identifier.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny,
                It.IsAny<IList<GamePlatform>>()))
            .Returns(platform);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(identifier.Object);
        var detector = new Mock<GameDetectorBase>(sp.Object, false);

        var options = new GameDetectorOptions(GameType.Foc)
        {
            TargetPlatforms = new List<GamePlatform>
            {
                GamePlatform.SteamGold
            }
        };

        detector.Setup(d => d.FindGameLocation(options))
            .Returns(new GameDetectorBase.GameLocationData { Location = fs.DirectoryInfo.New("Game") });


        var result = detector.Object.Detect(options);

        Assert.Null(result.GameLocation);
        Assert.Equal(type, result.GameIdentity.Type);
    }
}