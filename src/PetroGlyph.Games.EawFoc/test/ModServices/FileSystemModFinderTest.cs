using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices;

public class FileSystemModFinderTest
{
    private readonly FileSystemModFinder _service;
    private readonly Mock<ISteamGameHelpers> _steamHelper;
    private readonly MockFileSystem _fileSystem;

    public FileSystemModFinderTest()
    {
        var sc = new ServiceCollection();
        _steamHelper = new Mock<ISteamGameHelpers>();
        _fileSystem = new MockFileSystem();
        sc.AddTransient(_ => _steamHelper.Object);
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _service = new FileSystemModFinder(sc.BuildServiceProvider());
    }

    [Fact]
    public void GameNotExists_Throws()
    {
        var game = new Mock<IGame>();
        Assert.Throws<GameException>(() => _service.FindMods(game.Object));
    }

    [Fact]
    public void TestNoMods_Normal()
    {
        _fileSystem.AddDirectory("Game/Mods");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestNoMods_Normal_NoFolder()
    {
        _fileSystem.AddDirectory("Game");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestNoMods_Steam()
    {
        _fileSystem.AddDirectory("Lib/Game/Eaw/Mods");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("wsDir"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestOneMods_Normal()
    {
        _fileSystem.AddDirectory("Game/Mods/ModA");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);
        Assert.Equal(TestUtils.IsUnixLikePlatform ? "/Game/Mods/ModA" : "c:\\game\\mods\\moda", mod.Identifier);
        Assert.Equal(ModType.Default, mod.Type);
    }

    [Fact]
    public void TestTwoMods_Normal()
    {
        _fileSystem.AddDirectory("Game/Mods/ModA");
        _fileSystem.AddDirectory("Game/Mods/ModB");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        Assert.Equal(2, mods.Count);
    }

    [Fact]
    public void TestOneDefaultMod_Steam()
    {
        _fileSystem.AddDirectory("Lib/Game/Eaw/Mods/ModA");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("path"));
        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);
        Assert.Equal(TestUtils.IsUnixLikePlatform ? "/Lib/Game/Eaw/Mods/ModA" : "c:\\lib\\game\\eaw\\mods\\moda",
            mod.Identifier);
        Assert.Equal(ModType.Default, mod.Type);
    }

    [Fact]
    public void TestOneDefaultModOneWsMod_Steam()
    {
        _fileSystem.AddDirectory("Lib/Game/Eaw/Mods/ModA");
        _fileSystem.AddDirectory("Lib/workshop/content/32470/12345678");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/workshop/content/32470/"));

        var mods = _service.FindMods(game.Object);
        Assert.Equal(2, mods.Count);
    }

    [Fact]
    public void TestOneWsMod_Steam()
    {
        _fileSystem.AddDirectory("Lib/workshop/content/32470/12345678");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Lib/workshop/content/32470/"));

        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);
        Assert.Equal("12345678", mod.Identifier);
        Assert.Equal(ModType.Workshops, mod.Type);
    }
}