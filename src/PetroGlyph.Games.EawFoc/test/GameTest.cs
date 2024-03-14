using System;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameTest
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGame(null!, null!, null!, null!));
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGame(id, null!, null!, null!));
        var fs = new MockFileSystem();
        var loc = fs.DirectoryInfo.New("Game");
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGame(id, loc, null!, null!));
        var nameEmpty = string.Empty;
        var name = "Name";
        Assert.Throws<ArgumentException>(() => new PetroglyphStarWarsGame(id, loc, nameEmpty, new Mock<IServiceProvider>().Object));
        Assert.Throws<ArgumentNullException>(() => new PetroglyphStarWarsGame(id, loc, name, null!));
    }

    [Fact]
    public void AddRemoveMods()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        Assert.Equal(0, game.Mods.Count);

        var modMock = new Mock<IMod>();
        modMock.Setup(m => m.Game).Returns(game);
        modMock.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        var modA = modMock.Object;

        game.AddMod(modA);
        Assert.Equal(1, game.Mods.Count);
        game.AddMod(modA);
        Assert.Equal(1, game.Mods.Count);
        Assert.Single(game);

        game.RemoveMod(modA);
        Assert.Equal(0, game.Mods.Count);

        game.RemoveMod(modA);
        Assert.Equal(0, game.Mods.Count);
        Assert.Empty(game);
    }

    [Fact]
    public void AddInvalidMod_Throws()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        var modMock = new Mock<IMod>();

        Assert.Throws<ModException>(() => game.AddMod(modMock.Object));
    }

    [Fact]
    public void AddModRaiseEvent()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        var raised = false;
        game.ModsCollectionModified += (_, args) =>
        {
            raised = true;
            Assert.Equal(ModCollectionChangedAction.Add, args.Action);
        };

        var modMock = new Mock<IMod>();
        modMock.Setup(m => m.Game).Returns(game);
        game.AddMod(modMock.Object);

        Assert.True(raised);
    }


    [Fact]
    public void RemoveModRaiseEvent()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        modMock.Setup(m => m.Game).Returns(game);
        game.AddMod(modA);

        var raised = false;
        game.ModsCollectionModified += (_, args) =>
        {
            raised = true;
            Assert.Equal(ModCollectionChangedAction.Remove, args.Action);
        };

        game.RemoveMod(modA);
        Assert.True(raised);
    }

    [Fact]
    public void FindMod()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var builder = new Mock<IModIdentifierBuilder>();
        sp.Setup(p => p.GetService(typeof(IModIdentifierBuilder))).Returns(builder.Object);
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Equals(It.IsAny<IModReference>())).Returns(true);
        modMock.Setup(m => m.Game).Returns(game);
        game.AddMod(modA);

        var mod = game.FindMod(modA);
        Assert.NotNull(mod);

        Assert.True(game.TryFindMod(modA, out _));
    }

    [Fact]
    public void FindMod_Throws()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var builder = new Mock<IModIdentifierBuilder>();
        builder.Setup(b => b.Normalize(It.IsAny<IModReference>()))
            .Returns(new ModReference("bla", ModType.Virtual));
        sp.Setup(p => p.GetService(typeof(IModIdentifierBuilder))).Returns(builder.Object);
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Game).Returns(game);
        game.AddMod(modA);

        Assert.Throws<ModNotFoundException>(() => game.FindMod(modA));
        Assert.False(game.TryFindMod(modA, out _));
    }

    [Fact]
    public void TestEquals()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var iF = new GameIdentity(GameType.Foc, GamePlatform.SteamGold);
        var ig = new GameIdentity(GameType.EaW, GamePlatform.Disk);
        var loc = fs.DirectoryInfo.New("Game");
        var locC = fs.DirectoryInfo.New("Game/");
        var locE = fs.DirectoryInfo.New("Dir");
        var name = "Name";
        var gameA = new PetroglyphStarWarsGame(id, loc, name, sp.Object);
        var gameB = new PetroglyphStarWarsGame(id, loc, name, sp.Object);
        var gameC = new PetroglyphStarWarsGame(id, locC, name, sp.Object);
        var gameD = new PetroglyphStarWarsGame(id, loc, "OtherName", sp.Object);
        var gameE = new PetroglyphStarWarsGame(id, locE, name, sp.Object);
        var gameF = new PetroglyphStarWarsGame(iF, loc, name, sp.Object);
        var gameG = new PetroglyphStarWarsGame(ig, loc, name, sp.Object);

        Assert.Equal((IGame)gameA, gameB);
        Assert.Equal((IGame)gameA, gameC);
        Assert.Equal((IGame)gameA, gameD);
        Assert.Equal((IGame)gameA, gameD);
        Assert.NotEqual((IGame)gameA, gameE);
        Assert.NotEqual((IGame)gameA, gameF);
        Assert.NotEqual((IGame)gameA, gameG);
    }

    [Fact]
    public void GetModDirTest()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithSubdirectory("Game");
        var sp = new Mock<IServiceProvider>();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var loc = fs.DirectoryInfo.New("Game");
        var name = "Name";
        var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);
        var dataLocation = game.ModsLocation;
        Assert.Equal(fs.Path.GetFullPath("Game/Mods"), dataLocation.FullName);
    }
}