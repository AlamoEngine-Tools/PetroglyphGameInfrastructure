using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test;

public class GameExecutableFileServiceTest
{
    private readonly GameExecutableFileService _service;

    private readonly Mock<IGameExecutableNameBuilder> _nameBuilder;

    public GameExecutableFileServiceTest()
    {
        var sc = new ServiceCollection();
        _nameBuilder = new Mock<IGameExecutableNameBuilder>();
        sc.AddTransient(_ => _nameBuilder.Object);
        _service = new GameExecutableFileService(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestInvalidName()
    {
        var game = new Mock<IGame>();
        _nameBuilder.Setup(b => b.GetExecutableFileName(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns(string.Empty);
        var exeFile = _service.GetExecutableForGame(game.Object, GameBuildType.Debug);
        Assert.Null(exeFile);
    }

    [Fact]
    public void TestFileNotExists()
    {
        var game = new Mock<IGame>();
        _nameBuilder.Setup(b => b.GetExecutableFileName(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns("test.exe");

        var fs = new MockFileSystem();
        fs.Initialize();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("."));

        var exeFile = _service.GetExecutableForGame(game.Object, GameBuildType.Debug);
        Assert.Null(exeFile);
    }

    [Fact]
    public void TestExists()
    {
        var game = new Mock<IGame>();
        _nameBuilder.Setup(b => b.GetExecutableFileName(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns("test.exe");

        var fs = new MockFileSystem();
        fs.Initialize().WithFile("test.exe");
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("."));

        var exeFile = _service.GetExecutableForGame(game.Object, GameBuildType.Debug);
        Assert.NotNull(exeFile);
    }
}