using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.Test;

public class SteamGameClientTest
{
    private readonly SteamGameClient _service;
    private readonly Mock<ISteamWrapper> _steam;
    private readonly Mock<IGame> _game;
    private readonly Mock<IGameProcessLauncher> _launcher;


    public SteamGameClientTest()
    {
        var fs = new MockFileSystem();
        fs.AddFile("test.exe", new MockFileData(string.Empty));
        var sc = new ServiceCollection();
        _steam = new Mock<ISteamWrapper>();
        sc.AddTransient(_ => _steam.Object);
        var fileService = new Mock<IGameExecutableFileService>();
        fileService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(fs.FileInfo.New("test.exe"));
        sc.AddTransient(_ => fileService.Object);
        
        _launcher = new Mock<IGameProcessLauncher>();
        sc.AddTransient(_ => _launcher.Object);

        _service = new SteamGameClient(sc.BuildServiceProvider());

        _game = new Mock<IGame>();
    }

    [Fact]
    public void TestNotCompatiblePlatform_Throws()
    {
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        _game.Setup(g => g.Game).Returns(_game.Object);
        Assert.Throws<GameStartException>(() => _service.Play(_game.Object));
    }

    [Fact]
    public void TestSteamNotRunning_Throws()
    {
        _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        _game.Setup(g => g.Game).Returns(_game.Object);
        Assert.Throws<GameStartException>(() => _service.Play(_game.Object));
    }

    [Fact]
    public void TestProcessCreated()
    {
        _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        _game.Setup(g => g.Game).Returns(_game.Object);
        _steam.Setup(s => s.IsRunning).Returns(true);

        var process = new Mock<IGameProcess>();
        process.Setup(p => p.State).Returns(GameProcessState.Closed);

        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Returns(process.Object);

        var actualProcess = _service.Play(_game.Object);
        Assert.Equal(process.Object.State, actualProcess.State);

    }
}