using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec.Steam;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModFinderTest : CommonTestBase
{
    private readonly ModFinder _modFinder;
    private readonly ISteamGameHelpers _steamGameHelpers;

    public ModFinderTest()
    {
        _modFinder = new ModFinder(ServiceProvider);
        _steamGameHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
    }

    [Fact]
    public void FindMods_NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _modFinder.FindMods(null!));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_GameNotExists_Throws(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        game.Directory.Delete(true);
        Assert.Throws<GameException>(() => _modFinder.FindMods(game));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_NoModsDirectory_ShouldNotFindMods(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        game.ModsLocation.Delete(true);

        if (game.Platform is GamePlatform.SteamGold)
        {
            var wsDir = _steamGameHelpers.GetWorkshopsLocation(game);
            wsDir.Delete(true);
        }

        var mods = _modFinder.FindMods(game);
        Assert.Empty(mods);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_EmptyModsDirectory_ShouldNotFindMods(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        game.ModsLocation.Create();

        if (game.Platform is GamePlatform.SteamGold)
        {
            var wsDir = _steamGameHelpers.GetWorkshopsLocation(game);
            wsDir.Create();
        }

        var mods = _modFinder.FindMods(game);
        Assert.Empty(mods);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithoutModinfo(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var installedMods = _modFinder.FindMods(game);

        var foundMod = Assert.Single(installedMods);

        Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
        Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);
        Assert.Equal(expectedMod.ModInfo, foundMod.Modinfo);

        // Cannot assert on expectedMod.Identifier, cause this test framework builds the wrong identifiers.
        Assert.Equal(expectedMod.Directory.Name, foundMod.ModReference.Identifier);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithInvalidModinfo(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallInvalidModinfoFile();


        var installedMods = _modFinder.FindMods(game);

        var foundMod = Assert.Single(installedMods);

        Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
        Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);
        Assert.Equal(expectedMod.ModInfo, foundMod.Modinfo);

        // Cannot assert on expectedMod.Identifier, cause this test framework builds the wrong identifiers.
        Assert.Equal(expectedMod.Directory.Name, foundMod.ModReference.Identifier);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithOneInvalidModinfoVariant(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallModinfoFile(new ModinfoData("Variant1"), "variant1");
        expectedMod.InstallInvalidModinfoFile("variant2");


        var installedMods = _modFinder.FindMods(game);

        var foundMod = Assert.Single(installedMods);

        Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
        Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);
        Assert.Equal("Variant1", foundMod.Modinfo!.Name);

        // Cannot assert on expectedMod.Identifier, cause this test framework builds the wrong identifiers.
        Assert.Equal($"{expectedMod.Directory.Name}:Variant1", foundMod.ModReference.Identifier);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithMainModinfo(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var modinfoData = new ModinfoData("MyMod");
        var expectedMod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modinfoData, ServiceProvider);
        expectedMod.InstallModinfoFile(modinfoData);

        var installedMods = _modFinder.FindMods(game);

        var foundMod = Assert.Single(installedMods);

        Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
        Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);

        // Cannot assert on expectedMod.Identifier, cause this test framework builds the wrong identifiers.
        Assert.Equal(expectedMod.Directory.Name, foundMod.ModReference.Identifier);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_WithOnlyManyVariantModinfos(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var info1 = new ModinfoData("MyName1");
        var info2 = new ModinfoData("MyName2");
        var expectedMod = game.InstallMod("DirName", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallModinfoFile(info1, "variant1");
        expectedMod.InstallModinfoFile(info2, "variant2");

        var installedMods = _modFinder.FindMods(game);

        Assert.Equal(2, installedMods.Count);

        foreach (var foundMod in installedMods)
        {
            Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
            Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);
        }

        Assert.Equivalent(new List<string>{ "MyName1", "MyName2" }, installedMods.Select(x => x.Modinfo!.Name), true);
        Assert.Equivalent(
            new List<string> { $"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2" },
            installedMods.Select(x => x.ModReference.Identifier), true);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_WithMainAndManyVariantModinfos(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var main = new ModinfoData("Main");
        var info1 = new ModinfoData("MyName1");
        var info2 = new ModinfoData("MyName2");
        var expectedMod = game.InstallMod("DirName", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallModinfoFile(main);
        expectedMod.InstallModinfoFile(info1, "variant1");
        expectedMod.InstallModinfoFile(info2, "variant2");

        var installedMods = _modFinder.FindMods(game);

        Assert.Equal(3, installedMods.Count);

        foreach (var foundMod in installedMods)
        {
            Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
            Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);
        }

        Assert.Equivalent(new List<string> { "Main", "MyName1", "MyName2" }, installedMods.Select(x => x.Modinfo!.Name),
            true);

        Assert.Equivalent(
            new List<string> { expectedMod.Directory.Name, $"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2" },
            installedMods.Select(x => x.ModReference.Identifier), true);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_Steam_ShouldAddWorkshopsAndMods(GameType type)
    {
        var game = FileSystem.InstallGame(new GameIdentity(type, GamePlatform.SteamGold), ServiceProvider);

        var steamMod = game.InstallMod("SteamMod", true, ServiceProvider);

        var modinfo = new ModinfoData("Name");
        var defaultMod = game.InstallMod(false, modinfo, ServiceProvider);
        defaultMod.InstallModinfoFile(modinfo);


        var installedMods = _modFinder.FindMods(game);

        Assert.Equal(2, installedMods.Count);

        Assert.Contains(installedMods,
            x => x.ModReference.Type == steamMod.Type && steamMod.Directory.FullName.Equals(x.Directory.FullName) &&
                 x.Modinfo is null && x.ModReference.Identifier.Equals(steamMod.Directory.Name));

        Assert.Contains(installedMods, x => x.ModReference.Type == defaultMod.Type &&
                                            defaultMod.Directory.FullName.Equals(x.Directory.FullName)
                                            && x.Modinfo is not null && x.Modinfo.Name.Equals(defaultMod.Name) &&
                                            x.ModReference.Identifier.Equals(defaultMod.Directory.Name));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_Steam_ShouldNotContainModOfWrongGame(GameType type)
    {
        var game = FileSystem.InstallGame(new GameIdentity(type, GamePlatform.SteamGold), ServiceProvider);

        var oppositeGameType = type is GameType.Eaw ? GameType.Foc : GameType.Eaw;

        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title",
            [$"{oppositeGameType.ToString().ToUpper()}"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        
        var defaultMod = game.InstallMod(true, modinfo, ServiceProvider);
        defaultMod.InstallModinfoFile(modinfo);

        Assert.Empty(_modFinder.FindMods(game));
    }
}