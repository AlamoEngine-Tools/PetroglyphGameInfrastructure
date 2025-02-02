using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Steam;
using AET.Modinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.TestingUtilities;
using Semver;
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
        Assert.Throws<ArgumentNullException>(() => _modFinder.FindMods(null!, FileSystem.Directory.CreateDirectory("path")));
        var game = FileSystem.InstallGame(CreateRandomGameIdentity(), ServiceProvider);
        Assert.Throws<ArgumentNullException>(() => _modFinder.FindMods(game, null!));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_GameNotExists_Throws(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        game.Directory.Delete(true);
        Assert.Throws<GameException>(() => _modFinder.FindMods(game));
        Assert.Throws<GameException>(() => _modFinder.FindMods(game, FileSystem.DirectoryInfo.New("path")));
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

        var modsFromDir = _modFinder.FindMods(game, game.ModsLocation);
        Assert.Empty(modsFromDir);
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
        AssertSingleFoundMod(installedMods, expectedMod, expectedMod.Directory.Name, null);

        var foundModsFromDir = _modFinder.FindMods(game, expectedMod.Directory);
        AssertSingleFoundMod(foundModsFromDir, expectedMod, expectedMod.Directory.Name, null);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithInvalidModinfo(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallInvalidModinfoFile();

        var installedMods = _modFinder.FindMods(game);
        AssertSingleFoundMod(installedMods, expectedMod, expectedMod.Directory.Name, null);

        var foundModsFromDir = _modFinder.FindMods(game, expectedMod.Directory);
        AssertSingleFoundMod(foundModsFromDir, expectedMod, expectedMod.Directory.Name, null);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_OneMod_WithOneInvalidModinfoVariant(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        var expectedModinfo = new ModinfoData("Variant1");

        expectedMod.InstallModinfoFile(expectedModinfo, "variant1");
        expectedMod.InstallInvalidModinfoFile("variant2");

        var installedMods = _modFinder.FindMods(game);
        AssertSingleFoundMod(installedMods, expectedMod, $"{expectedMod.Directory.Name}:Variant1", expectedModinfo);

        var foundModsFromDir = _modFinder.FindMods(game, expectedMod.Directory);
        AssertSingleFoundMod(foundModsFromDir, expectedMod, $"{expectedMod.Directory.Name}:Variant1", expectedModinfo);
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
        AssertSingleFoundMod(installedMods, expectedMod, expectedMod.Directory.Name, modinfoData);

        var foundModsFromDir = _modFinder.FindMods(game, expectedMod.Directory);
        AssertSingleFoundMod(foundModsFromDir, expectedMod, expectedMod.Directory.Name, modinfoData);
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

        var installedMods = _modFinder.FindMods(game).ToList();
        AssertMultipleModsOfSameLocation(
            installedMods,
            2,
            expectedMod.Directory.FullName,
            expectedMod.Type,
            [$"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2"],
            ["MyName1", "MyName2"]);

        Assert.Equivalent(new List<string>{ "MyName1", "MyName2" }, installedMods.Select(x => x.Modinfo!.Name), true);

        var installedModsFromDir = _modFinder.FindMods(game, expectedMod.Directory).ToList();
        AssertMultipleModsOfSameLocation(
            installedModsFromDir,
            2,
            expectedMod.Directory.FullName,
            expectedMod.Type,
            [$"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2"],
            ["MyName1", "MyName2"]);
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

        var installedMods = _modFinder.FindMods(game).ToList();
        AssertMultipleModsOfSameLocation(
            installedMods,
            3,
            expectedMod.Directory.FullName,
            expectedMod.Type,
            [expectedMod.Directory.Name, $"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2"],
            ["Main", "MyName1", "MyName2"]);

        var installedModsFromDir = _modFinder.FindMods(game, expectedMod.Directory).ToList();
        AssertMultipleModsOfSameLocation(
            installedModsFromDir,
            3,
            expectedMod.Directory.FullName,
            expectedMod.Type,
            [expectedMod.Directory.Name, $"{expectedMod.Directory.Name}:MyName1", $"{expectedMod.Directory.Name}:MyName2"],
            ["Main", "MyName1", "MyName2"]);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_FindAllInstalledMods(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var mod1 = game.InstallMod("Mod1", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        var mod2 = game.InstallMod("Mod2", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var expectedDirs = new[] {mod1.Directory.FullName, mod2.Directory.FullName};
        var expectedIds = new[] {mod1.Directory.Name, mod2.Directory.Name};

        var installedMods = _modFinder.FindMods(game).ToList();

        Assert.Equal(2, installedMods.Count);
        Assert.Equivalent(expectedDirs, installedMods.Select(x => x.Directory.FullName), true);
        Assert.Equivalent(expectedIds, installedMods.Select(x => x.ModReference.Identifier), true);
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


        var installedMods = _modFinder.FindMods(game).ToList();

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

        
        var modOfWrongGameType = game.InstallMod(true, modinfo, ServiceProvider);
        modOfWrongGameType.InstallModinfoFile(modinfo);

        Assert.Empty(_modFinder.FindMods(game));
        Assert.Empty(_modFinder.FindMods(game, modOfWrongGameType.Directory));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_FromExternal(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var modPath = FileSystem.DirectoryInfo.New("external/myMod");
        modPath.Create();

        var mod = game.InstallMod(modPath, false, new ModinfoData("MyMod"), ServiceProvider);
        
        var installedMods = _modFinder.FindMods(game, mod.Directory).ToList();
        AssertSingleFoundMod(installedMods, mod, mod.Directory.FullName, null);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_FromExternal_DirectoryNotFoundShouldSkip(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var modPath = FileSystem.DirectoryInfo.New("external/myMod");
        modPath.Create();

        var mod = game.InstallMod(modPath, false, new ModinfoData("MyMod"), ServiceProvider);
        modPath.Delete(true);

        var installedMods = _modFinder.FindMods(game, mod.Directory).ToList();
        Assert.Empty(installedMods);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_FromExternal_WithVariantModinfoLayout(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var modPath = FileSystem.DirectoryInfo.New("external/123456"); // Use a number so it may look like a Steam WS ID
        modPath.Create();

        var main = new ModinfoData("Main");
        var info1 = new ModinfoData("MyName1");
        var info2 = new ModinfoData("MyName2");
        var expectedMod = game.InstallMod(modPath, false, main, ServiceProvider);
        expectedMod.InstallModinfoFile(main);
        expectedMod.InstallModinfoFile(info1, "variant1");
        expectedMod.InstallModinfoFile(info2, "variant2");

        var baseId = expectedMod.Directory.FullName;

        var installedMods = _modFinder.FindMods(game, expectedMod.Directory).ToList();
        AssertMultipleModsOfSameLocation(installedMods, 3, expectedMod.Directory.FullName, ModType.Default, 
            [$"{baseId}", $"{baseId}:MyName1", $"{baseId}:MyName2"], 
            ["Main", "MyName1", "MyName2"]);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_FromExternal_InsideSteamWsDirWithNonIdName(GameType type)
    {
        var game = FileSystem.InstallGame(new GameIdentity(type, GamePlatform.SteamGold), ServiceProvider);
        var steamHelper = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var steamLocation = steamHelper.GetWorkshopsLocation(game);
        steamLocation.Create();

        var modPath = FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(steamLocation.FullName, "notASteamID"));
        modPath.Create();

        var expectedMod = game.InstallMod(modPath, false, new ModinfoData("MyMod"), ServiceProvider);
        Assert.Equal(ModType.Default ,expectedMod.Type);
        
        var installedMods = _modFinder.FindMods(game, expectedMod.Directory).ToList();
        AssertSingleFoundMod(installedMods, expectedMod, modPath.FullName, null);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_ModInsideSteamWsDirWithNonIdName_ShouldBeSkipped(GameType type)
    {
        var game = FileSystem.InstallGame(new GameIdentity(type, GamePlatform.SteamGold), ServiceProvider);
        var steamHelper = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var steamLocation = steamHelper.GetWorkshopsLocation(game);
        steamLocation.Create();

        var modPath = FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(steamLocation.FullName, "notASteamID"));
        modPath.Create();

        game.InstallMod(modPath, false, new ModinfoData("MyMod"), ServiceProvider);

        var installedMods = _modFinder.FindMods(game).ToList();
        Assert.Empty(installedMods);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_ModInstalledInWrongGameModsDirectoryShouldBeSkipped(GameType type)
    {
        var oppositeGameType = type is GameType.Eaw ? GameType.Foc : GameType.Eaw;
        var game = FileSystem.InstallGame(new GameIdentity(type, TestHelpers.GetRandom(GITestUtilities.RealPlatforms)), ServiceProvider);
        // Other, random platform to shuffle a bit more.
        var otherTypeGame = FileSystem.InstallGame(new GameIdentity(oppositeGameType, TestHelpers.GetRandom(GITestUtilities.RealPlatforms)), ServiceProvider);

        var wrongMod = otherTypeGame.InstallMod("MyMod", false, ServiceProvider);

        var installedMods = _modFinder.FindMods(game, wrongMod.Directory).ToList();
        Assert.Empty(installedMods);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void FindMods_NoSteamWsDirectoryExistsShouldStillFindExternalMods(GameType type)
    {
        var game = FileSystem.InstallGame(new GameIdentity(type, GamePlatform.SteamGold), ServiceProvider);
        var steamHelper = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var steamLocation = steamHelper.GetWorkshopsLocation(game);
        steamLocation.Delete(true);

        var modPath = FileSystem.DirectoryInfo.New("external/MyMod");
        modPath.Create();

        game.InstallMod(modPath, false, new ModinfoData("MyMod"), ServiceProvider);

        var installedMods = _modFinder.FindMods(game).ToList();
        Assert.Empty(installedMods);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void FindMods_InvalidModinfoContentIsSkippedButModIsFound(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var expectedMod = game.InstallMod("MyMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        expectedMod.InstallModinfoFile(new CustomModinfo(string.Empty)); // string.Empty is not valid

        var installedMods = _modFinder.FindMods(game);
        var expectedId = expectedMod.Type == ModType.Workshops ? ((ulong)"MyMod".GetHashCode()).ToString() : "MyMod";

        AssertSingleFoundMod(installedMods, expectedMod, expectedId, null);
    }

    private static void AssertSingleFoundMod(IEnumerable<DetectedModReference> foundMods, IPhysicalMod expectedMod, string expectedId, IModinfo? expectedModinfo)
    {
        var foundMod = Assert.Single(foundMods);

        Assert.Equal(expectedMod.Directory.FullName, foundMod.Directory.FullName);
        Assert.Equal(expectedMod.Type, foundMod.ModReference.Type);

        Assert.Equal<IModIdentity>(expectedModinfo, foundMod.Modinfo);
        if (expectedModinfo is not null)
            Assert.Equal(expectedModinfo.Name, foundMod.Modinfo!.Name);

        Assert.Equal(expectedId, foundMod.ModReference.Identifier);
    }

    private static void AssertMultipleModsOfSameLocation(
        ICollection<DetectedModReference> detectedMods,
        int expectedCount,
        string expectedModsPath,
        ModType expectedModsType,
        ICollection<string> expectedIdentifiers,
        ICollection<string> expectedModinfoNames)
    {
        Assert.Equal(expectedCount, detectedMods.Count);

        Assert.All(detectedMods, x =>
        {
            Assert.Equal(expectedModsPath, x.Directory.FullName);
            Assert.Equal(expectedModsType, x.ModReference.Type);
        });

        Assert.Equivalent(expectedIdentifiers, detectedMods.Select(x => x.ModReference.Identifier), true);

        Assert.Equivalent(expectedModinfoNames, detectedMods.Select(x => x.Modinfo?.Name), true);
    }

    private class CustomModinfo(string name) : IModinfo
    {
        public bool Equals(IModIdentity? other)
        {
            throw new NotImplementedException();
        }

        public string Name { get; } = name;
        public SemVersion? Version { get; }
        public IModDependencyList Dependencies { get; init; } = DependencyList.EmptyDependencyList;
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
        }

        public void ToJson(Stream stream)
        {
            JsonSerializer.Serialize(stream, this, JsonSerializerOptions.Default);
        }

        public string? Summary { get; }
        public string? Icon { get; }
        public IDictionary<string, object> Custom { get; } = new Dictionary<string, object>();
        public ISteamData? SteamData { get; }
        public IReadOnlyCollection<ILanguageInfo> Languages { get; } = new List<ILanguageInfo>();
        public bool LanguagesExplicitlySet { get; }
    }
}