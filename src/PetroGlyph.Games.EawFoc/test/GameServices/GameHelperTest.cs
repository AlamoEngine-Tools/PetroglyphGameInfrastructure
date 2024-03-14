using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

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
        _fileSystem.Initialize()
            .WithSubdirectory("SteamLib/Apps/common/32470/Game")
            .WithSubdirectory("workshop/content/32470");

        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("SteamLib/Apps/common/32470/Game"));
        mock.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        var wsDir = _service.GetWorkshopsLocation(mock.Object);
        Assert.Equal(_fileSystem.Path.GetFullPath("SteamLib/Apps/workshop/content/32470"), wsDir.FullName);
    }

    [Fact]
    public void GetWorkshopDir_FailNotExisting()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game");
        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        mock.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        Assert.Throws<SteamException>(() => _service.GetWorkshopsLocation(mock.Object));
    }

    [Fact]
    public void GetWorkshopDir_FailNoSteam()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game");
        var mock = new Mock<IGame>();
        mock.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        Assert.Throws<GameException>(() => _service.GetWorkshopsLocation(mock.Object));
    }
}