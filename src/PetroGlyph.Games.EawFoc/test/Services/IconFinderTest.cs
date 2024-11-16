using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Utilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class IconFinderTest : CommonTestBase
{
    private readonly IconFinder _iconFinder = new();

    [Fact]
    public void FindIcon_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _iconFinder.FindIcon(null!));
    }
    
    [Fact]
    public void FindIcon_Game_NotInstalled()
    {
        var game = CreateRandomGame();
        Assert.Null(_iconFinder.FindIcon(game));
    }


    [Fact]
    public void FindIcon_Game_Installed()
    {
        var game = CreateRandomGame();
        var expectedFileName = game.Type == GameType.Eaw ? "eaw.ico" : "foc.ico";
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "foc.ico"));

        var icon = _iconFinder.FindIcon(game);
        Assert.NotNull(icon);
        Assert.Equal(
            FileSystem.Path.GetFullPath(FileSystem.Path.Combine(game.Directory.FullName, expectedFileName)),
            FileSystem.Path.GetFullPath(icon));
    }

    [Fact]
    public void FindIcon_Game_NotInstalledWrongLocation()
    {
        var game = CreateRandomGame();
        game.DataDirectory().Create();
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "Data", "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "Data", "foc.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "eaw.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "foc.ic"));
        Assert.Null(_iconFinder.FindIcon(game));
    }

    [Fact]
    public void FindIcon_Mod_NotInstalled()
    {
        var game = CreateRandomGame();
        var mod = game.InstallAndAddMod("Mod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "Data", "icon.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "icon.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "icon.ic"));
        Assert.Null(_iconFinder.FindIcon(mod));
    }

    [Fact]
    public void FindIcon_Mod_Installed()
    {
        var game = CreateRandomGame();
        var mod = game.InstallAndAddMod("Mod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var icon1 = $"{FileSystem.Path.GetRandomFileName()}.ico";
        var icon2 = $"{FileSystem.Path.GetRandomFileName()}.ico";

        var icons = new List<string> { icon1, icon2 };

        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon1));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon2));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        // The method does not specify which file is found if multiple exists
        Assert.Contains(FileSystem.Path.GetFileName(foundIcon), icons);
    }

    [Fact]
    public void FindIcon_Mod_UseIconFromFs_ModinfoIconIsNull()
    {
        var game = CreateRandomGame();
        var mod = game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), new ModinfoData("name"), ServiceProvider);

        var icon1 = $"{FileSystem.Path.GetRandomFileName()}.ico";
        var icon2 = $"{FileSystem.Path.GetRandomFileName()}.ico";

        var icons = new List<string> { icon1, icon2 };

        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon1));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon2));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        // The method does not specify which file is found if multiple exists
        Assert.Contains(FileSystem.Path.GetFileName(foundIcon), icons);
    }

    [Fact]
    public void FindIcon_Mod_UseIconFromFs_ModinfoIconIsEmpty()
    {
        var game = CreateRandomGame();
        var mod = game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), new ModinfoData("name")
        {
            Icon = string.Empty
        }, ServiceProvider);

        var icon1 = $"{FileSystem.Path.GetRandomFileName()}.ico";
        var icon2 = $"{FileSystem.Path.GetRandomFileName()}.ico";

        var icons = new List<string> { icon1, icon2 };

        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon1));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon2));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        // The method does not specify which file is found if multiple exists
        Assert.Contains(FileSystem.Path.GetFileName(foundIcon), icons);
    }

    [Fact]
    public void FindIcon_Mod_UseIconFromModinfo()
    {
        var game = CreateRandomGame();
        var mod = game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), new ModinfoData("name")
        {
            Icon = "icon.ico"
        }, ServiceProvider);

        var icon1 = $"{FileSystem.Path.GetRandomFileName()}.ico";
        var icon2 = $"{FileSystem.Path.GetRandomFileName()}.ico";
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon1));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, icon2));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        Assert.Equal("icon.ico", foundIcon);
    }

    [Fact]
    public void FindIcon_Mod_UseIconFromGame()
    {
        var game = CreateRandomGame();
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "foc.ico"));
        var expectedFileName = game.Type == GameType.Eaw ? "eaw.ico" : "foc.ico";

        var mod = game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), new ModinfoData("name"), ServiceProvider);

        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "Data", "notAnIcon.ico"));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        // The method does not specify which file is found if multiple exists
        Assert.Equal(expectedFileName, FileSystem.Path.GetFileName(foundIcon));
    }

    [Fact]
    public void FindIcon_VirtualMod_UseIconFromModinfo()
    {
        var game = CreateRandomGame();
        var dep = game.InstallAndAddMod("dep", false, ServiceProvider);
        var mod = new VirtualMod(game,
            new ModinfoData("Mod")
            {
                Icon = "icon.ico",
                Dependencies = new DependencyList(new List<IModReference>
                {
                    new ModReference(dep)
                }, DependencyResolveLayout.FullResolved)
            }, ServiceProvider);

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        Assert.Equal("icon.ico", foundIcon);
    }
}