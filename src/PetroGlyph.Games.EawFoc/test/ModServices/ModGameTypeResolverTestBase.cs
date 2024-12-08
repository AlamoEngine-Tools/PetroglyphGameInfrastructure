using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
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

public abstract class ModGameTypeResolverTestBase : CommonTestBase
{
    public abstract IModGameTypeResolver CreateResolver();

    [Fact]
    public void TryGetGameType_Modinfo_IsNull_CannotGetType()
    {
        Assert.False(CreateResolver().TryGetGameType(null!, out _));
    }

    [Fact]
    public void TryGetGameType_Modinfo_WithoutSteamData_CannotGetType()
    {
        var modinfo = new ModinfoData("Name");
        Assert.False(CreateResolver().TryGetGameType(modinfo, out _));
    }

    [Theory]
    [InlineData]
    [InlineData("tag")]
    [InlineData("tag", "notFoc")]
    [InlineData("tag", "notFoc", "notEAW")]
    public void TryGetGameType_WithoutSteamGameTag_CannotGetType(params string[] tags)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title", tags);

        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };
        Assert.False(CreateResolver().TryGetGameType(modinfo, out _));
    }

    public static IEnumerable<object[]> GetSteamTagsSuccessTestData()
    {
        yield return [new List<string> { "EAW" }, new List<GameType> { GameType.Eaw }];
        yield return [new List<string> { "eaW" }, new List<GameType> { GameType.Eaw }];
        yield return [new List<string> { "FOC" }, new List<GameType> { GameType.Foc }];
        yield return [new List<string> { "foc" }, new List<GameType> { GameType.Foc }];
        yield return [new List<string> { "foc", "EAW" }, new List<GameType> { GameType.Eaw, GameType.Foc }];
        yield return [new List<string> { "other", "foc", "EAW" }, new List<GameType> { GameType.Eaw, GameType.Foc }];
    }

    [Theory]
    [MemberData(nameof(GetSteamTagsSuccessTestData))]
    public void TryGetGameType_Modinfo_WithSteamData(IList<string> tags, ICollection<GameType> expectedTypes)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title", tags);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        Assert.True(CreateResolver().TryGetGameType(modinfo, out var types));
        Assert.Equivalent(expectedTypes, types, true);
    }

    [Fact]
    public void TryGetGameType_Directory_WithModinfo_ModinfoIsAlwaysSuperiorForSteamTypes()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title",
            ["EAW"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        // Installing FOC, while tags has EAW. So technically, EAW does not even exist.
        var game = FileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        // Using a known MODID (RaW), which is FOC, to ensure cache does not hit.
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory("1129810972");

        Assert.True(CreateResolver().TryGetGameType(modDir, ModType.Workshops, modinfo, out var types));
        var type = Assert.Single(types);
        Assert.Equal(GameType.Eaw, type);
    }

    [Fact]
    public void TryGetGameType_Directory_SteamWithoutModinfoCannotDecide()
    {
        // No SteamData here
        var modinfo = new ModinfoData("Name");

        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), ServiceProvider);

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();

        // Using an ID here, which never points to a real mod.
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory("1234");

        var steamMod = game.InstallMod(modDir, true, modinfo, ServiceProvider);

        // We cannot say for sure to which game a mod belongs if it is in WS directory.
        Assert.False(CreateResolver().TryGetGameType(steamMod.Directory, ModType.Workshops, modinfo, out var types)); 
        Assert.Empty(types);
    }

    [Fact]
    public void TryGetGameType_Directory_SteamWithInvalidDirName()
    {
        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), ServiceProvider);
        
        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory("notASteamId");

        var steamMod = game.InstallMod(modDir, true, new ModinfoData("Name"), ServiceProvider);

        Assert.False(CreateResolver().TryGetGameType(steamMod.Directory, ModType.Workshops, null, out var types));
        Assert.Empty(types);
    }

    public static IEnumerable<object[]> GetCachedModsTestData()
    {
        foreach (var knownMod in KnownSteamWorkshopCache.KnownMods)
        {
            yield return [knownMod.Key.ToString(), knownMod.Value.Types];
        }
    }

    [Theory]
    [MemberData(nameof(GetCachedModsTestData))]
    public void TryGetGameType_Directory_SteamWithoutModinfoButKnownModID(string knownId, ICollection<GameType> expectedTypes)
    {
        // No SteamData here
        var modinfo = new ModinfoData("Name");

        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), ServiceProvider);

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();

        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory(knownId);

        // We cannot say for sure to which game a mod belongs if it is in WS directory.
        Assert.True(CreateResolver().TryGetGameType(modDir, ModType.Workshops, modinfo, out var types));
        Assert.Equivalent(expectedTypes, types, true);
    }

    [Fact]
    public void TryGetGameType_VirtualMods_VirtualModsAreAlwaysUndecidable()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var game = FileSystem.InstallGame(CreateRandomGameIdentity(), ServiceProvider);
        var mod = game.InstallMod("Name", false, ServiceProvider);

        // We have a installed mod, and modinfo, but still virtual mods shall never return a confirmed value
        Assert.False(CreateResolver().TryGetGameType(mod.Directory, ModType.Virtual, modinfo, out var types)); 
        Assert.Empty(types);
    }

    [Fact]
    public void TryGetGameType_ExternalModsAreAlwaysUndecidable()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var modDir = FileSystem.Directory.CreateDirectory("/ExternalMod");

        // SteamData from modinfo should not be used. "External mods" are undecidable
        Assert.False(CreateResolver().TryGetGameType(modDir, ModType.Default, modinfo, out var types));
        Assert.Empty(types);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void TryGetGameType_ModsInModsDirUseGameType(GameType gameType)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            TestHelpers.GetRandomEnum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var game = FileSystem.InstallGame(new GameIdentity(gameType, TestHelpers.GetRandom(GITestUtilities.RealPlatforms)), ServiceProvider);
        var mod = game.InstallMod("Name", false, ServiceProvider);

        Assert.True(CreateResolver().TryGetGameType(mod.Directory, ModType.Default, modinfo, out var types));
        var type = Assert.Single(types);
        Assert.Equal(game.Type, type);
    }
}