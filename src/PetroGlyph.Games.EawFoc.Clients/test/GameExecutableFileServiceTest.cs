using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test;

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
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("."));

        var exeFile = _service.GetExecutableForGame(game.Object, GameBuildType.Debug);
        Assert.Null(exeFile);
    }

    [Fact]
    public void TestExists()
    {
#if NET
        return;
#endif
        var game = new Mock<IGame>();
        _nameBuilder.Setup(b => b.GetExecutableFileName(It.IsAny<IGame>(), It.IsAny<GameBuildType>()))
            .Returns("test.exe");

        var fs = new MockFileSystem();
        fs.AddFile("test.exe", MockFileData.NullObject);
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("."));

        var exeFile = _service.GetExecutableForGame(game.Object, GameBuildType.Debug);
        Assert.NotNull(exeFile);
    }
}