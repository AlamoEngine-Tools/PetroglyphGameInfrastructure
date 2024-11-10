using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class PlayableModContainerTest : PlayableObjectTest
{
    protected abstract PlayableModContainer CreateModContainer();

    [Fact]
    public void Mods_NoMods()
    {
        var obj = CreateModContainer();
        Assert.Empty(obj.Mods);
    }

    [Fact]
    public void AddMod_RemoveMod()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod("mod", false, ServiceProvider);
        var modSame = game.InstallMod("mod", false, ServiceProvider);

        Assert.Empty(container.Mods);

        Assert.True(container.AddMod(mod));
        Assert.Single(container.Mods);
        Assert.False(container.AddMod(mod));
        Assert.False(container.AddMod(modSame));
        Assert.Single(container.Mods);
        Assert.Single(container);

        Assert.True(container.RemoveMod(modSame));
        Assert.Empty(container.Mods);
        Assert.False(container.RemoveMod(mod));

        Assert.Empty(container.Mods);
        Assert.Empty(container);
    }
    
    //[Fact]
    //public void AddInvalidMod_Throws()
    //{
    //    var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
    //    var otherGame = new PetroglyphStarWarsGame(gameIdentity, game.Directory, game.Name, ServiceProvider);

    //    var mod = otherGame.InstallMod("mod", false, ServiceProvider);

    //    Assert.Throws<ModException>(() => game.AddMod(mod));

    //    if (gameIdentity.Platform == GamePlatform.SteamGold)
    //    {
    //        var wsMod = otherGame.InstallMod("steamMod", true, ServiceProvider);
    //        Assert.Throws<ModException>(() => game.AddMod(mod));
    //    }
    //}

    //[Fact]
    //public void AddModRaiseEvent()
    //{
    //    var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

    //    var raised = false;
    //    game.ModsCollectionModified += (_, args) =>
    //    {
    //        raised = true;
    //        Assert.Equal(ModCollectionChangedAction.Add, args.Action);
    //    };

    //    var modMock = new Mock<IMod>();
    //    modMock.Setup(m => m.Game).Returns(game);
    //    game.AddMod(modMock.Object);

    //    Assert.True(raised);
    //}


    //[Fact]
    //public void RemoveModRaiseEvent()
    //{
    //    var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

    //    var modMock = new Mock<IMod>();
    //    var modA = modMock.Object;
    //    modMock.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
    //    modMock.Setup(m => m.Game).Returns(game);
    //    game.AddMod(modA);

    //    var raised = false;
    //    game.ModsCollectionModified += (_, args) =>
    //    {
    //        raised = true;
    //        Assert.Equal(ModCollectionChangedAction.Remove, args.Action);
    //    };

    //    game.RemoveMod(modA);
    //    Assert.True(raised);
    //}

    //[Fact]
    //public void FindMod()
    //{
    //    var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

    //    var modMock = new Mock<IMod>();
    //    var modA = modMock.Object;
    //    modMock.Setup(m => m.Equals(It.IsAny<IModReference>())).Returns(true);
    //    modMock.Setup(m => m.Game).Returns(game);
    //    game.AddMod(modA);

    //    var mod = game.FindMod(modA);
    //    Assert.NotNull(mod);
    //}
}