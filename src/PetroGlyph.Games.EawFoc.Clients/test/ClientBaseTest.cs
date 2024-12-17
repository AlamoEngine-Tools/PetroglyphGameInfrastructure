using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test;

public class ClientBaseTest
{
    private readonly Mock<ClientBase> _client;
    private readonly Mock<IGame> _game;
    private readonly Mock<IModArgumentListFactory> _modListFactory;
    private readonly Mock<IGameExecutableFileService> _exeService;
    private readonly Mock<IGameProcessLauncher> _launcher;


    public ClientBaseTest()
    {
        var sc = new ServiceCollection();
        _modListFactory = new Mock<IModArgumentListFactory>();
        _exeService = new Mock<IGameExecutableFileService>();
        _launcher = new Mock<IGameProcessLauncher>();
        sc.AddSingleton(_ => _modListFactory.Object);
        sc.AddSingleton(_ => _exeService.Object);
        sc.AddSingleton(_ => _launcher.Object);
        _client = new Mock<ClientBase>(sc.BuildServiceProvider());
        _client.Setup(c => c.GetGameLauncherService()).Returns(_launcher.Object);
        _game = new Mock<IGame>();
    }

    [Fact]
    public void TestPlatformNotSupported_Throws()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments).Returns(ArgumentCollection.Empty);
            
        var e = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.StartsWith("Unable to start", e.Message);
    }

    [Fact]
    public void TestPlatformOnGameStartingCancels_Throws()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments).Returns(ArgumentCollection.Empty);
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), ArgumentCollection.Empty, GameBuildType.Release)).Returns(false);

        var e = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.Equal("Game starting was aborted.", e.Message);
    }

    [Fact]
    public void TestPlatformOnGameStartingEventCancel_Throws()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments).Returns(ArgumentCollection.Empty);
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), ArgumentCollection.Empty, GameBuildType.Release)).Returns(true);

        var handlerCalled = false;
        _client.Object.GameStarting += (sender, args) =>
        {
            Assert.Empty(args.GameArguments);
            Assert.Equal(GameBuildType.Release, args.BuildType);
            args.Cancel = true;
            handlerCalled = true;
        };

        var e = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.Equal("Game starting was aborted.", e.Message);
        Assert.True(handlerCalled);
    }

    [Fact]
    public void TestExeNotFound_Throws()
    { 
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments).Returns(ArgumentCollection.Empty);
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), ArgumentCollection.Empty, GameBuildType.Release)).Returns(true);

        var e1 = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.StartsWith("Executable for", e1.Message);

        var fs = new MockFileSystem();

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("test.exe"));

        var e2 = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.StartsWith("Executable for", e2.Message);
    }

    [Fact]
    public void TesGameNotStarted_Throws()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments).Returns(ArgumentCollection.Empty);
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), ArgumentCollection.Empty, GameBuildType.Release)).Returns(true);

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("test.exe"));

        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Throws(new GameStartException(_game.Object, "Test message"));

        var e2 = Assert.Throws<GameStartException>(() => _client.Object.Play(_game.Object));
        Assert.Equal("Test message", e2.Message);
    }

    [Fact]
    public void TestGameStarted()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments)
            .Returns(new ArgumentCollection(new List<IGameArgument> { new WindowedArgument() }));
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), It.IsAny<IArgumentCollection>(), GameBuildType.Release)).Returns(true);

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("test.exe"));

        var process = new Mock<IGameProcess>(); 
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Callback((IFileInfo _, GameProcessInfo g) =>
            {
                var arg = Assert.Single(g.Arguments);
                Assert.Equal(new WindowedArgument(), arg);
                process.Setup(p => p.ProcessInfo).Returns(g);
                process.Setup(p => p.State).Returns(GameProcessState.Running);
            })
            .Returns(process.Object);


        _client.Setup(c => c.OnGameStarted(It.IsAny<IGameProcess>()))
            .Callback((IGameProcess p) =>
            {
                Assert.Equal(GameProcessState.Running, p.State);
            });


        var gameStarted = false;
        _client.Object.GameStarted += (_, gameProcess) =>
        {
            Assert.Equal(gameProcess.ProcessInfo, process.Object.ProcessInfo);
            gameStarted = true;
        };
           
            
        var actualProcess = _client.Object.Play(_game.Object);

        Assert.Equal(actualProcess.ProcessInfo, process.Object.ProcessInfo);
        Assert.True(gameStarted);
        _launcher.Verify(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()), Times.Exactly(1));
        _client.Verify(l => l.OnGameStarted(It.IsAny<IGameProcess>()), Times.Exactly(1));
    }

    [Fact]
    public void TestRunningInstanceListUpdates()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments)
            .Returns(new ArgumentCollection(new List<IGameArgument> { new WindowedArgument() }));
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), It.IsAny<IArgumentCollection>(), GameBuildType.Release)).Returns(true);

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("test.exe"));

        var process = new Mock<IGameProcess>();
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Returns(process.Object);


        Assert.Empty(_client.Object.RunningInstances); 
        _client.Object.Play(_game.Object);
        Assert.Single(_client.Object.RunningInstances);

        var closed = false;
        _client.Object.GameClosed += (_, gameProcess) =>
        {
            Assert.Equal(gameProcess.ProcessInfo, process.Object.ProcessInfo);
            closed = true;
        };

        process.Raise(p => p.Closed += null, EventArgs.Empty);


        Assert.True(closed);
        Assert.Empty(_client.Object.RunningInstances);
    }

    [Fact]
    public void TestGameStartedButClosedWhileInHandler()
    {
        _game.Setup(g => g.Game).Returns(_game.Object);
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _client.Setup(c => c.SupportedPlatforms).Returns(new HashSet<GamePlatform> { GamePlatform.Disk });
        _client.Setup(c => c.DefaultArguments)
            .Returns(new ArgumentCollection(new List<IGameArgument> { new WindowedArgument() }));
        _client.Setup(c => c.OnGameStarting(It.IsAny<IGame>(), It.IsAny<IArgumentCollection>(), GameBuildType.Release)).Returns(true);

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");

        _exeService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), GameBuildType.Release))
            .Returns(fs.FileInfo.New("test.exe"));

        var process = new Mock<IGameProcess>();
        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Returns(process.Object);


        _client.Setup(c => c.OnGameStarted(It.IsAny<IGameProcess>()))
            .Callback(() =>
            {
                process.Setup(p => p.State).Returns(GameProcessState.Closed);
            });


        var gameStarted = false;
        _client.Object.GameStarted += (_, _) =>
        {
            gameStarted = true;
        };


        var actualProcess = _client.Object.Play(_game.Object);

        Assert.Equal(actualProcess.ProcessInfo, process.Object.ProcessInfo);
        Assert.False(gameStarted);
        _client.Verify(l => l.OnGameStarted(It.IsAny<IGameProcess>()), Times.Exactly(1));
        Assert.Empty(_client.Object.RunningInstances);
    }
}