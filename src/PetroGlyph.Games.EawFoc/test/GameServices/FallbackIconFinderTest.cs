﻿using System.Collections.Generic;
using System.IO.Abstractions;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.FileService;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class FallbackIconFinderTest
{
    [Fact]
    public void TestMissing()
    {
        var fs = new MockFileSystem();
        var fileService = new Mock<IPhysicalFileService>();
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));
        game.Setup(g => g.FileService).Returns(fileService.Object);
        var finder = new FallbackGameIconFinder();
        Assert.Null(finder.FindIcon(game.Object));
    }

    [Fact]
    public void TestWrongIcon()
    {
        var fs = new MockFileSystem();
        var fileService = new Mock<IPhysicalFileService>();
        fileService.Setup(f => f.DataFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.Foc);
        game.Setup(g => g.FileService).Returns(fileService.Object);
        var finder = new FallbackGameIconFinder();
        Assert.Null(finder.FindIcon(game.Object));
    }

    [Fact]
    public void TestFocIcon()
    {
        var fs = new MockFileSystem();
        var fileService = new Mock<IPhysicalFileService>();
        fileService.Setup(f => f.DataFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
        {
            fs.FileInfo.New("Game/foc.ico")
        });
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.Foc);
        game.Setup(g => g.FileService).Returns(fileService.Object);
        var finder = new FallbackGameIconFinder();
        var icon = finder.FindIcon(game.Object);
        Assert.Equal(fs.Path.GetFullPath("Game/foc.ico"), icon);
    }

    [Fact]
    public void TestEawIcon()
    {
        var fs = new MockFileSystem();
        var fileService = new Mock<IPhysicalFileService>();
        fileService.Setup(f => f.DataFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
        {
            fs.FileInfo.New("Game/eaw.ico")
        });
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));
        game.Setup(g => g.Type).Returns(GameType.EaW);
        game.Setup(g => g.FileService).Returns(fileService.Object);
        var finder = new FallbackGameIconFinder();
        var icon = finder.FindIcon(game.Object);
        Assert.Equal(fs.Path.GetFullPath("Game/eaw.ico"), icon);
    }
}