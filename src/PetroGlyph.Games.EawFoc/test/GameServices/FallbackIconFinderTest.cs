using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Utilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class FallbackIconFinderTest
{
    private readonly Mock<IPlayableObjectFileService> _fileService = new();
    private readonly FallbackGameIconFinder _iconFinder;
    private readonly MockFileSystem _fileSystem = new();

    public FallbackIconFinderTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton(_fileService.Object);
        _iconFinder = new FallbackGameIconFinder(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestMissing()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        Assert.Null(_iconFinder.FindIcon(game.Object));
    }

    [Fact]
    public void TestWrongIcon()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.Foc);
        _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());
        Assert.Null(_iconFinder.FindIcon(game.Object));
    }

    [Fact]
    public void TestFocIcon()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.Foc);
        _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
        {
            _fileSystem.FileInfo.New("Game/foc.ico")
        });
        var icon = _iconFinder.FindIcon(game.Object);
        Assert.Equal(_fileSystem.Path.GetFullPath("Game/foc.ico"), icon);
    }

    [Fact]
    public void TestEawIcon()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.Eaw);
        _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
        {
            _fileSystem.FileInfo.New("Game/eaw.ico")
        });
        var icon = _iconFinder.FindIcon(game.Object);
        Assert.Equal(_fileSystem.Path.GetFullPath("Game/eaw.ico"), icon);
    }
}