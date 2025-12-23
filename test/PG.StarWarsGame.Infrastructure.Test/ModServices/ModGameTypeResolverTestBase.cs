using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Steam;
using AET.Modinfo.Utilities;
using AET.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public abstract class ModGameTypeResolverTestBase : GameInfrastructureTestBase
{
    public abstract ModGameTypeResolver CreateResolver();

    protected static DetectedModReference CreateDetectedModReference(IDirectoryInfo dir, ModType type, IModinfo? modinfo)
    {
        return new DetectedModReference(new ModReference("SOME_ID", type), dir, modinfo);
    }

    [Fact]
    public void NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateResolver().TryGetGameType((DetectedModReference)null!, out _));
        Assert.Throws<ArgumentNullException>(() => CreateResolver().IsDefinitelyNotCompatibleToGame((DetectedModReference)null!, Random.Enum<GameType>()));
    }

    [Fact]
    public void Modinfo_IsNull_CannotGetType()
    {
        var resolver = CreateResolver();
        Assert.False(resolver.TryGetGameType((IModinfo?)null!, out _));
        Assert.False(resolver.IsDefinitelyNotCompatibleToGame((IModinfo?)null!, Random.Enum<GameType>()));
    }

    [Fact]
    public void Modinfo_WithoutSteamData_CannotGetType()
    {
        var modinfo = new ModinfoData("Name");

        var resolver = CreateResolver();
        Assert.False(resolver.TryGetGameType(modinfo, out _));
        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(modinfo, Random.Enum<GameType>()));
    }

    [Theory]
    [InlineData]
    [InlineData("tag")]
    [InlineData("tag", "notFoc")]
    [InlineData("tag", "notFoc", "notEAW")]
    public void Modinfo_WithoutSteamGameTag_CannotGetType(params string[] tags)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title", tags);

        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var resolver = CreateResolver();
        Assert.False(resolver.TryGetGameType(modinfo, out _));
        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(modinfo, Random.Enum<GameType>()));
    }

    public static IEnumerable<object[]> GetSteamTagsSuccessTestData()
    {
        yield return [new List<string> { "EAW" }, new[] { GameType.Eaw }, GameType.Foc];
        yield return [new List<string> { "eaW" }, new[] { GameType.Eaw }, GameType.Foc];
        yield return [new List<string> { "FOC" }, new[] { GameType.Foc }, GameType.Eaw];
        yield return [new List<string> { "foc" }, new[] { GameType.Foc }, GameType.Eaw];
        yield return [new List<string> { "foc", "EAW" }, new[] { GameType.Eaw, GameType.Foc }, null!];
        yield return [new List<string> { "other", "foc", "EAW" }, new[] { GameType.Eaw, GameType.Foc }, null!];
    }

    [Theory]
    [MemberData(nameof(GetSteamTagsSuccessTestData))]
    public void Modinfo_WithSteamDataAndSteamTag_CanDetermineGameType(IList<string> tags, ICollection<GameType> expectedTypes, GameType? incompatibleWith)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title", tags);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var resolver = CreateResolver();
        Assert.True(resolver.TryGetGameType(modinfo, out var types));
        Assert.Equivalent(expectedTypes, types, true);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(modinfo, Random.Item(expectedTypes)));
        if (incompatibleWith is not null)
            Assert.True(resolver.IsDefinitelyNotCompatibleToGame(modinfo, incompatibleWith.Value));

    }

    [Fact]
    public void Directory_WithModinfo_ModinfoIsAlwaysSuperiorForSteamTypes()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["EAW"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        // Installing FOC, while tags has EAW. So technically, EAW does not even exist.
        var game = GetOrCreateGameInstallation(new GameIdentity(GameType.Foc, GamePlatform.SteamGold)).Game;
        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        // Using a known MODID (RaW), which is FOC, to implicitly assert cache is not used.
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory("1129810972");

        var info = CreateDetectedModReference(modDir, ModType.Workshops, modinfo);
        var resolver = CreateResolver();

        Assert.True(resolver.TryGetGameType(info, out var types));
        var type = Assert.Single(types);
        Assert.Equal(GameType.Eaw, type);

        Assert.True(resolver.IsDefinitelyNotCompatibleToGame(info, GameType.Foc));
        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, GameType.Eaw));
    }

    [Fact]
    public void Directory_SteamWithoutModinfoCannotDecide()
    {
        // No SteamData here
        var modinfo = new ModinfoData("Name");

        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(Random.Enum<GameType>(), GamePlatform.SteamGold));

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();

        // Using an ID here, which never points to a real mod.
        var modDir = steamHelpers.GetWorkshopsLocation(gameInstallation.Game).CreateSubdirectory("1234");

        var steamMod = gameInstallation.InstallMod(modinfo, modDir, true).Mod;

        var info = CreateDetectedModReference(steamMod.Directory, ModType.Workshops, modinfo);
        var resolver = CreateResolver();

        // We cannot say for sure to which game a mod belongs if it is in WS directory.
        Assert.False(resolver.TryGetGameType(info, out var types)); 
        Assert.Empty(types);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, Random.Enum<GameType>()));
    }

    [Fact]
    public void Directory_SteamWithInvalidDirName()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(Random.Enum<GameType>(), GamePlatform.SteamGold));
        
        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var modDir = steamHelpers.GetWorkshopsLocation(gameInstallation.Game).CreateSubdirectory("notASteamId");

        var steamMod = gameInstallation.InstallMod(modinfo, modDir, true).Mod;

        // This asserts that we do not use steam data from modinfo if the directory is not a valid steam workshops id
        var info = CreateDetectedModReference(steamMod.Directory, ModType.Workshops, modinfo);
        var resolver = CreateResolver();

        Assert.False(resolver.TryGetGameType(info, out var types));
        Assert.Empty(types);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, Random.Enum<GameType>()));
    }

    public static IEnumerable<object[]> GetCachedModsTestData()
    {
        foreach (var knownMod in KnownSteamWorkshopCache.KnownMods)
        {
            GameType? incompatible;
            if (knownMod.Value.Types.Length == 2)
                incompatible = null;
            else
                incompatible = knownMod.Value.Types.First().Opposite();

            yield return [knownMod.Key.ToString(), knownMod.Value.Types, incompatible!];
        }
    }

    [Theory]
    [MemberData(nameof(GetCachedModsTestData))]
    public void Directory_SteamWithoutModinfoButKnownModID(string knownId, ICollection<GameType> expectedTypes, GameType? incompatibleWith)
    {
        // No SteamData here
        var modinfo = new ModinfoData("Name");

        var game = GetOrCreateGameInstallation(new GameIdentity(Random.Enum<GameType>(), GamePlatform.SteamGold)).Game;

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();

        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory(knownId);

        var info = CreateDetectedModReference(modDir, ModType.Workshops, modinfo);
        var resolver = CreateResolver();

        Assert.True(resolver.TryGetGameType(info, out var types));
        Assert.Equivalent(expectedTypes, types, true);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, Random.Item(expectedTypes)));
        if (incompatibleWith is not null)
            Assert.True(resolver.IsDefinitelyNotCompatibleToGame(info, incompatibleWith.Value));
    }

    [Fact]
    public void VirtualMods_VirtualModsAreAlwaysUndecidable()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var mod = GetOrCreateGameInstallation().InstallMod("Name", false).Mod;

        var info = CreateDetectedModReference(mod.Directory, ModType.Virtual, modinfo);
        var resolver = CreateResolver();

        // We have an installed mod, and modinfo, but still virtual mods shall never return a confirmed value
        Assert.False(CreateResolver().TryGetGameType(info, out var types)); 
        Assert.Empty(types);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, Random.Enum<GameType>()));
    }

    [Fact]
    public void ExternalModsAreAlwaysUndecidable()
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var modDir = FileSystem.Directory.CreateDirectory("/ExternalMod");

        var info = CreateDetectedModReference(modDir, ModType.Default, modinfo);
        var resolver = CreateResolver();

        // SteamData from modinfo should not be used. "External mods" are undecidable
        Assert.False(CreateResolver().TryGetGameType(info, out var types));
        Assert.Empty(types);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, Random.Enum<GameType>()));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void ModsInModsDirUseGameType(GameType gameType)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, Random.Item(GITestUtilities.RealPlatforms)));
        var game = gameInstallation.Game;
        var mod = gameInstallation.InstallMod("Name", false).Mod;

        var info = CreateDetectedModReference(mod.Directory, ModType.Default, modinfo);
        var resolver = CreateResolver();

        Assert.True(resolver.TryGetGameType(info, out var types));
        var type = Assert.Single(types);
        Assert.Equal(game.Type, type);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, game.Type));
        Assert.True(resolver.IsDefinitelyNotCompatibleToGame(info, game.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw));
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void ModsNotInModsDirButSomeOtherGameBasesDir_NotDecidable(GameType gameType)
    {
        var steamData = new SteamData(
            new Random().Next(0, int.MaxValue).ToString(),
            "path",
            Random.Enum<SteamWorkshopVisibility>(),
            "Title",
            ["FOC"]);
        var modinfo = new ModinfoData("Name")
        {
            SteamData = steamData
        };

        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, Random.Item(GITestUtilities.RealPlatforms)));
        var game = gameInstallation.Game;
        var modDir = game.Directory.CreateSubdirectory("ModsOther").CreateSubdirectory("MyMod");
        var mod = gameInstallation.InstallMod(modinfo, modDir, false).Mod;

        var info = CreateDetectedModReference(mod.Directory, ModType.Default, modinfo);
        var resolver = CreateResolver();

        Assert.False(resolver.TryGetGameType(info, out var types));
        Assert.Empty(types);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, game.Type));
    }
}