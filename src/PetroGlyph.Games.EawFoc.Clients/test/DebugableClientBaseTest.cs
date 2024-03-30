using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test;

public class DebugableClientBaseTest
{
    private readonly TestDebugClient _service;
    private readonly Mock<IGameExecutableFileService> _exeService;
    private readonly Mock<IGameProcessLauncher> _launcher;

    public DebugableClientBaseTest()
    {
        var sc = new ServiceCollection();
        _exeService = new Mock<IGameExecutableFileService>();
        sc.AddTransient(_ => _exeService.Object);
        _launcher = new Mock<IGameProcessLauncher>();
        sc.AddTransient(_ => _launcher.Object);
        _service = new TestDebugClient(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestIsDebugAvailable()
    {
        var game = new Mock<IGame>();
        var fs = new MockFileSystem();

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns((IFileInfo)null!);
        Assert.False(_service.IsDebugAvailable(game.Object));
        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(fs.FileInfo.New("test.exe"));
        Assert.False(_service.IsDebugAvailable(game.Object));
        
        fs.Initialize().WithFile("test.exe");
        
        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(fs.FileInfo.New("test.exe"));
        Assert.True(_service.IsDebugAvailable(game.Object));
    }

    [Fact]
    public void TestDebugGame()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Game).Returns(game.Object);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("release.exe");

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Debug))
            .Returns(fs.FileInfo.New("debug.exe"));
        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("release.exe"));

        // Don't fallback - Throws.
        Assert.Throws<GameStartException>(() => _service.Debug(game.Object, ArgumentCollection.Empty, false));

        // Fallback to Release
        var releaseProcess = new Mock<IGameProcess>();
        releaseProcess.Setup(p => p.ProcessInfo)
            .Returns(new GameProcessInfo(game.Object, GameBuildType.Release, ArgumentCollection.Empty));
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>())).Returns(releaseProcess.Object);
        var realProcess = _service.Debug(game.Object, ArgumentCollection.Empty, true);
        Assert.Equal(GameBuildType.Release, realProcess.ProcessInfo.BuildType);

        // Use Debug
        fs.Initialize().WithFile("debug.exe");
        var debugProcess = new Mock<IGameProcess>();
        debugProcess.Setup(p => p.ProcessInfo)
            .Returns(new GameProcessInfo(game.Object, GameBuildType.Debug, ArgumentCollection.Empty));
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>())).Returns(debugProcess.Object);
        realProcess = _service.Debug(game.Object, ArgumentCollection.Empty, true);
        Assert.Equal(GameBuildType.Debug, realProcess.ProcessInfo.BuildType);

    }

    private class TestDebugClient(IServiceProvider serviceProvider) : DebugableClientBase(serviceProvider)
    {
        public override ISet<GamePlatform> SupportedPlatforms => new HashSet<GamePlatform> { GamePlatform.Disk };
        protected internal override IGameProcessLauncher GetGameLauncherService()
        {
            return ServiceProvider.GetRequiredService<IGameProcessLauncher>();
        }
    }
}