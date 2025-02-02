using System;
using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class LanguageFinderTest : CommonTestBaseWithRandomGame
{
    private readonly InstalledLanguageFinder _languageFinder;

    public LanguageFinderTest()
    {
        _languageFinder = new InstalledLanguageFinder(ServiceProvider);
    }

    private void InstallGameLanguage(LanguageInfo language) => Game.InstallLanguage(language);
    private void InstallModLanguage(IPhysicalMod mod, LanguageInfo language) => mod.InstallLanguage(language);

    [Fact]
    public void Ctor_NullArgTest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InstalledLanguageFinder(null!));
    }

    [Fact]
    public void FindLanguages_NullArgTest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InstalledLanguageFinder(null!));
    }

    [Fact]
    public void FindLanguages_Game_WithNoLanguages()
    {
        var langs = _languageFinder.FindLanguages(Game);
        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Game_WithLanguages_En_De()
    {
        InstallGameLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallGameLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));
        var langs = _languageFinder.FindLanguages(Game);

        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_WithNoLanguages_UsesGame()
    {
        InstallGameLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var mod = Game.InstallMod("myMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_WithLanguages_En_De_Steam()
    {
        var game = FileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
        var mod = game.InstallMod("myMod", true, ServiceProvider);
        InstallModLanguage(mod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(mod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetModDoesNotHaveLanguages()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseMod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetAndDependencyModDoNotHaveLanguages()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetModDoesNotHaveLanguagesAndDependencyIsNotResolved()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        // Do not resolve dependencies here!

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveDefaultLanguageInstalled()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseMod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        // Only english is installed
        InstallModLanguage(mod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        // We should not have the languages from base, because only english was detected as physically installed.
        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("en", LanguageSupportLevel.FullLocalized) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveModinfoLanguages()
    {

        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseMod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved),
            Languages = new List<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
                new LanguageInfo("de", LanguageSupportLevel.SFX)
            }
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        // Should not be considered
        InstallModLanguage(mod, new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        // We should not have the languages from base, because only english was detected as physically installed.
        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.SFX)

        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveModinfoWithDefaultLanguagesExplicitlySet()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseMod, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved),
            // Set language
            Languages = new List<ILanguageInfo> { LanguageInfo.Default }
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        // Should not be considered
        InstallModLanguage(mod, new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        // We should not have the languages from base, because only english was detected as physically installed.
        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesNotHaveLanguagesAndDependencyAlsoDoesNotHaveLanguages_UsesGameFallback()
    {
        InstallGameLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesNotHaveLanguagesAndTransitiveDependencyHasLanguages()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleModInfo = new ModinfoData("middleMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var middleMod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), middleModInfo, ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod)
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modInfo, ServiceProvider);
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_VirtualMod_TargetModDoesNotHaveLanguagesAndSecondDependencyHasLanguages()
    {
        var baseMod = Game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        InstallModLanguage(baseMod, new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleMod = Game.InstallAndAddMod("middleMod", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod), new ModReference(baseMod)
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = new VirtualMod(Game, "VirtualModId", modInfo, ServiceProvider);
        Game.AddMod(mod);
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }


    [Fact]
    public void FindLanguages_Mod_FromModinfo_WithNoLanguages()
    {
        var mod = Game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(Game), new ModinfoData("myMod"), ServiceProvider);
        var langs = _languageFinder.FindLanguages(mod);
        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }
}