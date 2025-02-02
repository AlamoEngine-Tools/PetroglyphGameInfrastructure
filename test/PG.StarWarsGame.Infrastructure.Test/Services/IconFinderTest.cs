using System;
using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.StarWarsGame.Infrastructure.Utilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class IconFinderTest : CommonTestBaseWithRandomGame
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
        Assert.Null(_iconFinder.FindIcon(Game));
    }

    [Fact]
    public void FindIcon_Game_Installed()
    {
        var expectedFileName = Game.Type == GameType.Eaw ? "eaw.ico" : "foc.ico";
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "foc.ico"));

        var icon = _iconFinder.FindIcon(Game);
        Assert.NotNull(icon);
        Assert.Equal(
            FileSystem.Path.GetFullPath(FileSystem.Path.Combine(Game.Directory.FullName, expectedFileName)),
            FileSystem.Path.GetFullPath(icon));
    }

    [Fact]
    public void FindIcon_Game_NotInstalledWrongLocation()
    {
        Game.DataDirectory().Create();
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "Data", "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "Data", "foc.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "eaw.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "foc.ic"));
        Assert.Null(_iconFinder.FindIcon(Game));
    }

    [Fact]
    public void FindIcon_Mod_NotInstalled()
    {
        var mod = Game.InstallAndAddMod("Mod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "Data", "icon.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "icon.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "icon.ic"));
        Assert.Null(_iconFinder.FindIcon(mod));
    }

    [Fact]
    public void FindIcon_Mod_Installed()
    {
        var mod = Game.InstallAndAddMod("Mod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);

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
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), new ModinfoData("name"), ServiceProvider);

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
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), new ModinfoData("name")
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
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), new ModinfoData("name")
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
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "eaw.ico"));
        FileSystem.File.Create(FileSystem.Path.Combine(Game.Directory.FullName, "foc.ico"));
        var expectedFileName = Game.Type == GameType.Eaw ? "eaw.ico" : "foc.ico";

        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), new ModinfoData("name"), ServiceProvider);

        FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, "Data", "notAnIcon.ico"));

        var foundIcon = _iconFinder.FindIcon(mod);
        Assert.NotNull(foundIcon);

        // The method does not specify which file is found if multiple exists
        Assert.Equal(expectedFileName, FileSystem.Path.GetFileName(foundIcon));
    }

    [Fact]
    public void FindIcon_VirtualMod_UseIconFromModinfo()
    {
        var dep = Game.InstallAndAddMod("dep", false, ServiceProvider);
        var mod = new VirtualMod(Game, "Mod",
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