using System.IO.Abstractions;
using AET.SteamAbstraction;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Steam;

public class SteamGameClientTest
{
    //private readonly SteamGameClient _service;
    //private readonly Mock<ISteamWrapperFactory> _steamFactory;
    //private readonly Mock<IGame> _game;
    //private readonly Mock<IGameProcessLauncher> _launcher;


    //public SteamGameClientTest()
    //{
    //    var fs = new MockFileSystem();
    //    fs.Initialize().WithFile("test.exe");
    //    var sc = new ServiceCollection();
    //    _steamFactory = new Mock<ISteamWrapperFactory>();
    //    sc.AddTransient(_ => _steamFactory.Object);
    //    var fileService = new Mock<IGameExecutableFileService>();
    //    fileService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
    //        .Returns(fs.FileInfo.New("test.exe"));
    //    sc.AddTransient(_ => fileService.Object);
        
    //    _launcher = new Mock<IGameProcessLauncher>();
    //    sc.AddTransient(_ => _launcher.Object);

    //    _service = new SteamGameClient(sc.BuildServiceProvider());

    //    _game = new Mock<IGame>();
    //}

    //[Fact]
    //public void TestNotCompatiblePlatform_Throws()
    //{
    //    _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
    //    _game.Setup(g => g.Game).Returns(_game.Object);
    //    Assert.Throws<GameStartException>(() => _service.Play(_game.Object));
    //}

    //[Fact]
    //public void TestSteamNotRunning_Throws()
    //{
    //    var steam = new Mock<ISteamWrapper>();
    //    steam.SetupGet(s => s.IsRunning).Returns(false);
    //    _steamFactory.Setup(s => s.CreateWrapper()).Returns(steam.Object);

    //    _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
    //    _game.Setup(g => g.Game).Returns(_game.Object);
    //    Assert.Throws<GameStartException>(() => _service.Play(_game.Object));
    //}

    //[Fact]
    //public void TestProcessCreated()
    //{
    //    var steam = new Mock<ISteamWrapper>();
    //    steam.SetupGet(s => s.IsRunning).Returns(true);
    //    _steamFactory.Setup(s => s.CreateWrapper()).Returns(steam.Object);

    //    _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
    //    _game.Setup(g => g.Game).Returns(_game.Object);

    //    var process = new Mock<IGameProcess>();
    //    process.Setup(p => p.State).Returns(GameProcessState.Closed);

    //    _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
    //        .Returns(process.Object);

    //    var actualProcess = _service.Play(_game.Object);
    //    Assert.Equal(process.Object.State, actualProcess.State);

    //}
}