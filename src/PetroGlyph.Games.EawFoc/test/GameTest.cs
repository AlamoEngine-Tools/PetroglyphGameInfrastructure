using System;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameTest : CommonTestBase
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        var id = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(null!, FileSystem.DirectoryInfo.New("path"), "name", ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, null!, "name", ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), "name", null!));
        Assert.Throws<ArgumentException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), "", ServiceProvider));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void AddRemoveMods(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var mod = game.InstallMod("mod", false, ServiceProvider);
        var modSame = game.InstallMod("mod", false, ServiceProvider);
        
        Assert.Empty(game.Mods);

        Assert.True(game.AddMod(mod));
        Assert.Single(game.Mods);
        Assert.False(game.AddMod(mod));
        Assert.False(game.AddMod(modSame));
        Assert.Single(game.Mods);
        Assert.Single(game);

        Assert.True(game.RemoveMod(modSame));
        Assert.Empty(game.Mods);
        Assert.False(game.RemoveMod(mod));

        Assert.Empty(game.Mods);
        Assert.Empty(game);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void AddInvalidMod_Throws(GameIdentity gameIdentity)
    { 
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var otherGame = new PetroglyphStarWarsGame(gameIdentity, game.Directory, game.Name, ServiceProvider);

        var mod = otherGame.InstallMod("mod", false, ServiceProvider);

        Assert.Throws<ModException>(() => game.AddMod(mod));

        if (gameIdentity.Platform == GamePlatform.SteamGold)
        {
            var wsMod = otherGame.InstallMod("steamMod", true, ServiceProvider);
            Assert.Throws<ModException>(() => game.AddMod(mod));
        }
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void AddModRaiseEvent(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

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


    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void RemoveModRaiseEvent(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

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

    // TODO:
    //[Theory]
    //[MemberData(nameof(RealGameIdentities))]
    //public void FindMod(GameIdentity gameIdentity)
    //{
    //    var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

    //    var modMock = new Mock<IMod>();
    //    var modA = modMock.Object;
    //    modMock.Setup(m => m.Equals(It.IsAny<IModReference>())).Returns(true);
    //    modMock.Setup(m => m.Game).Returns(game);
    //    game.AddMod(modA);

    //    var mod = game.FindMod(modA);
    //    Assert.NotNull(mod);

    //    Assert.True(game.TryFindMod(modA, out _));
    //}

    //[Fact]
    //public void FindMod_Throws()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var builder = new Mock<IModIdentifierBuilder>();
    //    builder.Setup(b => b.Normalize(It.IsAny<IModReference>()))
    //        .Returns(new ModReference("bla", ModType.Virtual));
    //    sp.Setup(p => p.GetService(typeof(IModIdentifierBuilder))).Returns(builder.Object);
    //    var id = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
    //    var loc = fs.DirectoryInfo.New("Game");
    //    var name = "Name";
    //    var game = new PetroglyphStarWarsGame(id, loc, name, sp.Object);

    //    var modMock = new Mock<IMod>();
    //    var modA = modMock.Object;
    //    modMock.Setup(m => m.Game).Returns(game);
    //    game.AddMod(modA);

    //    Assert.Throws<ModNotFoundException>(() => game.FindMod(modA));
    //    Assert.False(game.TryFindMod(modA, out _));
    //}

    //[Fact]
    //public void TestEquals()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var id = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
    //    var iF = new GameIdentity(GameType.Foc, GamePlatform.SteamGold);
    //    var ig = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
    //    var loc = fs.DirectoryInfo.New("Game");
    //    var locC = fs.DirectoryInfo.New("Game/");
    //    var locE = fs.DirectoryInfo.New("Dir");
    //    var name = "Name";
    //    var gameA = new PetroglyphStarWarsGame(id, loc, name, sp.Object);
    //    var gameB = new PetroglyphStarWarsGame(id, loc, name, sp.Object);
    //    var gameC = new PetroglyphStarWarsGame(id, locC, name, sp.Object);
    //    var gameD = new PetroglyphStarWarsGame(id, loc, "OtherName", sp.Object);
    //    var gameE = new PetroglyphStarWarsGame(id, locE, name, sp.Object);
    //    var gameF = new PetroglyphStarWarsGame(iF, loc, name, sp.Object);
    //    var gameG = new PetroglyphStarWarsGame(ig, loc, name, sp.Object);

    //    Assert.Equal((IGame)gameA, gameB);
    //    Assert.Equal((IGame)gameA, gameC);
    //    Assert.Equal((IGame)gameA, gameD);
    //    Assert.Equal((IGame)gameA, gameD);
    //    Assert.NotEqual((IGame)gameA, gameE);
    //    Assert.NotEqual((IGame)gameA, gameF);
    //    Assert.NotEqual((IGame)gameA, gameG);
    //}

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void GetModDirTest(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var dataLocation = game.ModsLocation;
        Assert.Equal(FileSystem.Path.GetFullPath(FileSystem.Path.Combine(game.Directory.FullName, "Mods")), dataLocation.FullName);
    }
}