using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test;

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
            .Returns((IFileInfo)null);
        Assert.False(_service.IsDebugAvailable(game.Object));
        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(fs.FileInfo.FromFileName("test.exe"));
        Assert.False(_service.IsDebugAvailable(game.Object));
        fs.AddFile("test.exe", MockFileData.NullObject);
        Assert.True(_service.IsDebugAvailable(game.Object));
    }

    [Fact]
    public void TestDebugGame()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Game).Returns(game.Object);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        var fs = new MockFileSystem();
        fs.AddFile("release.exe", MockFileData.NullObject);

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Debug))
            .Returns(fs.FileInfo.FromFileName("debug.exe"));
        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.FromFileName("release.exe"));

        // Don't fallback - Throws.
        Assert.Throws<GameStartException>(() => _service.Debug(game.Object, new EmptyArgumentsCollection(), false));

        // Fallback to Release
        var releaseProcess = new Mock<IGameProcess>();
        releaseProcess.Setup(p => p.ProcessInfo)
            .Returns(new GameProcessInfo(game.Object, GameBuildType.Release, new EmptyArgumentsCollection()));
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>())).Returns(releaseProcess.Object);
        var realProcess = _service.Debug(game.Object, new EmptyArgumentsCollection(), true);
        Assert.Equal(GameBuildType.Release, realProcess.ProcessInfo.BuildType);

        // Use Debug
        fs.AddFile("debug.exe", MockFileData.NullObject);
        var debugProcess = new Mock<IGameProcess>();
        debugProcess.Setup(p => p.ProcessInfo)
            .Returns(new GameProcessInfo(game.Object, GameBuildType.Debug, new EmptyArgumentsCollection()));
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>())).Returns(debugProcess.Object);
        realProcess = _service.Debug(game.Object, new EmptyArgumentsCollection(), true);
        Assert.Equal(GameBuildType.Debug, realProcess.ProcessInfo.BuildType);

    }


    private class TestDebugClient : DebugableClientBase
    {
        public TestDebugClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override ISet<GamePlatform> SupportedPlatforms => new HashSet<GamePlatform> { GamePlatform.Disk };
        protected override IGameProcessLauncher GetGameLauncherService()
        {
            return ServiceProvider.GetRequiredService<IGameProcessLauncher>();
        }
    }
}