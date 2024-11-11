using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Testing;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class IconFinderTest : CommonTestBase
{
    private readonly IconFinder _iconFinder = new();

    //[Fact]
    //public void TestMissing()
    //{
    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
    //    Assert.Null(_iconFinder.FindIcon(game.Object));
    //}

    //[Fact]
    //public void TestWrongIcon()
    //{
    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
    //    game.Setup(g => g.Type).Returns(GameType.Foc);
    //    _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());
    //    Assert.Null(_iconFinder.FindIcon(game.Object));
    //}

    //[Fact]
    //public void TestFocIcon()
    //{
    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
    //    game.Setup(g => g.Type).Returns(GameType.Foc);
    //    _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
    //    {
    //        _fileSystem.FileInfo.New("Game/foc.ico")
    //    });
    //    var icon = _iconFinder.FindIcon(game.Object);
    //    Assert.Equal(_fileSystem.Path.GetFullPath("Game/foc.ico"), icon);
    //}

    //[Fact]
    //public void TestEawIcon()
    //{
    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
    //    game.Setup(g => g.Type).Returns(GameType.Eaw);
    //    _fileService.Setup(f => f.DataFiles(game.Object, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>
    //    {
    //        _fileSystem.FileInfo.New("Game/eaw.ico")
    //    });
    //    var icon = _iconFinder.FindIcon(game.Object);
    //    Assert.Equal(_fileSystem.Path.GetFullPath("Game/eaw.ico"), icon);
    //}
}