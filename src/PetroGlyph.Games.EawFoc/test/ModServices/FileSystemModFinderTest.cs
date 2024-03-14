﻿using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Testably.Abstractions.Testing;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices;

public class FileSystemModFinderTest
{
    private readonly FileSystemModFinder _service;
    private readonly Mock<ISteamGameHelpers> _steamHelper;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IModIdentifierBuilder> _idBuilder;

    public FileSystemModFinderTest()
    {
        var sc = new ServiceCollection();
        _steamHelper = new Mock<ISteamGameHelpers>();
        _fileSystem = new MockFileSystem();
        _idBuilder = new Mock<IModIdentifierBuilder>();
        sc.AddTransient(_ => _steamHelper.Object);
        sc.AddSingleton<IFileSystem>(_ => _fileSystem);
        sc.AddSingleton(_ => _idBuilder.Object);
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
        _fileSystem.Initialize().WithSubdirectory("Game/Mods");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestNoMods_Normal_NoFolder()
    {
        _fileSystem.Initialize().WithSubdirectory("Game");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Game/Mods"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestNoMods_Steam()
    {
        _fileSystem.Initialize().WithSubdirectory("Lib/Game/Eaw/Mods");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.New("wsDir"));
        var mods = _service.FindMods(game.Object);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestOneMods_Normal()
    {
        _fileSystem.Initialize().WithSubdirectory("Game/Mods/ModA");
        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Game/Mods"));

        _idBuilder.Setup(ib => ib.Build(It.IsAny<IDirectoryInfo>(), false))
            .Returns("somePath");

        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);
        Assert.Equal("somePath", mod.Identifier);
        Assert.Equal(ModType.Default, mod.Type);
    }

    [Fact]
    public void TestTwoMods_Normal()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/ModA")
            .WithSubdirectory("Game/Mods/ModB");

        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Game/Mods"));

        _idBuilder.SetupSequence(ib => ib.Build(It.IsAny<IDirectoryInfo>(), false))
            .Returns("somePath1")
            .Returns("somePath2");

        var mods = _service.FindMods(game.Object);
        Assert.Equal(2, mods.Count);
    }

    [Fact]
    public void TestOneDefaultMod_Steam()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Lib/Game/Eaw/Mods/ModA");

        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.New("path"));

        _idBuilder.Setup(ib => ib.Build(It.IsAny<IDirectoryInfo>(), false))
            .Returns("builderPath");

        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);

        Assert.Equal("builderPath", mod.Identifier);
        Assert.Equal(ModType.Default, mod.Type);
    }

    [Fact]
    public void TestOneDefaultModOneWsMod_Steam()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Lib/Game/Eaw/Mods/ModA")
            .WithSubdirectory("Lib/workshop/content/32470/12345678");

        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.New("Lib/workshop/content/32470/"));

        _idBuilder.Setup(ib => ib.Build(It.IsAny<IDirectoryInfo>(), false))
            .Returns("defaultPath");
        _idBuilder.Setup(ib => ib.Build(It.IsAny<IDirectoryInfo>(), true))
            .Returns("workshopPath");

        var mods = _service.FindMods(game.Object);
        Assert.Equal(2, mods.Count);

        var wsMod = mods.First(m => m.Type == ModType.Workshops);
        Assert.Equal("workshopPath", wsMod.Identifier);

        var defaultMod = mods.First(m => m.Type == ModType.Default);
        Assert.Equal("defaultPath", defaultMod.Identifier);
    }

    [Fact]
    public void TestOneWsMod_Steam()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Lib/workshop/content/32470/12345678");

        var game = new Mock<IGame>();
        game.Setup(g => g.Exists()).Returns(true);
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        game.Setup(g => g.ModsLocation).Returns(_fileSystem.DirectoryInfo.New("Lib/Game/Eaw/Mods"));
        _steamHelper.Setup(h => h.GetWorkshopsLocation(game.Object))
            .Returns(_fileSystem.DirectoryInfo.New("Lib/workshop/content/32470/"));

        _idBuilder.Setup(ib => ib.Build(It.IsAny<IDirectoryInfo>(), true))
            .Returns("workshopPath");

        var mods = _service.FindMods(game.Object);
        var mod = Assert.Single(mods);
        Assert.Equal("workshopPath", mod.Identifier);
        Assert.Equal(ModType.Workshops, mod.Type);
    }
}