using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test;

public class DefaultClientTest
{
    private readonly DefaultClient _service;
    private readonly Mock<IGame> _game;
    private readonly Mock<IGameProcessLauncher> _launcher;


    public DefaultClientTest()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");
        var sc = new ServiceCollection();
        var fileService = new Mock<IGameExecutableFileService>();
        fileService.Setup(s => s.GetExecutableForGame(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(fs.FileInfo.New("test.exe"));
        sc.AddTransient(_ => fileService.Object);
        
        _launcher = new Mock<IGameProcessLauncher>();
        sc.AddTransient(_ => _launcher.Object);

        _game = new Mock<IGame>();

        _service = new DefaultClient(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestNotCompatiblePlatform_Throws()
    {
        _game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        _game.Setup(g => g.Game).Returns(_game.Object);
        Assert.Throws<GameStartException>(() => _service.Play(_game.Object));
    }

    [Fact]
    public void TestProcessCreated()
    {
        _game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        _game.Setup(g => g.Game).Returns(_game.Object);

        var process = new Mock<IGameProcess>();
        process.Setup(p => p.State).Returns(GameProcessState.Closed);

        _launcher.Setup(l => l.StartGameProcess(It.IsAny<IFileInfo>(), It.IsAny<GameProcessInfo>()))
            .Returns(process.Object);

        var actualProcess = _service.Play(_game.Object);
        Assert.Equal(process.Object.State, actualProcess.State);

    }
}