using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices;

public class GameHelperTest
{
    private readonly SteamGameHelpers _service;
    private readonly MockFileSystem _fileSystem;

    public GameHelperTest()
    {
        var sc = new ServiceCollection();
        _fileSystem = new MockFileSystem();
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _service = new SteamGameHelpers(sc.BuildServiceProvider());
    }

    [Fact]
    public void GetWorkshopDir_Success()
    {
        var fs = new MockFileSystem();
        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("SteamLib/Apps/common/32470/Game"));
        mock.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        fs.AddDirectory("SteamLib/Apps/common/32470/Game");
        fs.AddDirectory("workshop/content/32470");
        var wsDir = _service.GetWorkshopsLocation(mock.Object);
        Assert.Equal(
            TestUtils.IsUnixLikePlatform
                ? "/SteamLib/Apps/workshop/content/32470"
                : "C:\\SteamLib\\Apps\\workshop\\content\\32470", wsDir.FullName);
    }

    [Fact]
    public void GetWorkshopDir_FailNotExisting()
    {
        var fs = new MockFileSystem();
        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
        mock.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        fs.AddDirectory("Game");
        Assert.Throws<SteamException>(() => _service.GetWorkshopsLocation(mock.Object));
    }

    [Fact]
    public void GetWorkshopDir_FailNoSteam()
    {
        var fs = new MockFileSystem();
        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
        fs.AddDirectory("Game");
        Assert.Throws<GameException>(() => _service.GetWorkshopsLocation(mock.Object));
    }
}