using System;
using System.IO.Abstractions;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class RegistryGameDetectorTest
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        var registry = new Mock<IGameRegistry>();
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(null, null, false, null));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(registry.Object, null, false, sp.Object));
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(null, registry.Object, false, sp.Object));
        Assert.Throws<ArgumentNullException>(() => new RegistryGameDetector(registry.Object, registry.Object, false, null));
    }

    [Fact]
    public void TestNoGame()
    {
        var fs = new MockFileSystem();
        var registry = new Mock<IGameRegistry>();
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new RegistryGameDetector(registry.Object, registry.Object, false, sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.Null(result.Location);
    }

    [Fact]
    public void TestNotInitialized()
    {
        var fs = new MockFileSystem();
        var registry = new Mock<IGameRegistry>();
        registry.Setup(r => r.Exits).Returns(true);
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new RegistryGameDetector(registry.Object, registry.Object, false, sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.True(result.InitializationRequired);
    }

    [Fact]
    public void TestInstalled()
    {
        var fs = new MockFileSystem();
        var registry = new Mock<IGameRegistry>();
        registry.Setup(r => r.Exits).Returns(true);
        registry.Setup(r => r.Version).Returns(new Version(1, 0));
        registry.Setup(r => r.ExePath).Returns(fs.FileInfo.New("Game/sweaw.exe"));
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
        var detector = new RegistryGameDetector(registry.Object, registry.Object, false, sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        var result = detector.FindGameLocation(options);
        Assert.NotNull(result.Location);
    }
}