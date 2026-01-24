using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using System;
using System.Collections.Generic;
using Xunit;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class LanguageFinderTest : GameInfrastructureTestBaseWithRandomGame
{
    private readonly InstalledLanguageFinder _languageFinder;

    public LanguageFinderTest()
    {
        _languageFinder = new InstalledLanguageFinder(ServiceProvider);
    }

    private void InstallGameLanguage(LanguageInfo language) => GameInstallation.InstallLanguage(language);
    
    private void InstallModLanguage(ITestingPhysicalModInstallation modInstallation, LanguageInfo language) => modInstallation.InstallLanguage(language);

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
        var mod = GameInstallation.InstallMod("myMod").Mod;
        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_WithLanguages_En_De_Steam()
    {
        var game = GameInfrastructureTesting.Game(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
        var modInstallation = game.InstallMod("myMod", true);
        InstallModLanguage(modInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(modInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(modInstallation.Mod);

        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetModDoesNotHaveLanguages()
    {
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseModInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseModInstallation.Mod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = GameInstallation.InstallAndAddMod(modInfo).Mod;
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
        var baseMod = GameInstallation.InstallAndAddMod("baseMod").Mod;

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = GameInstallation.InstallAndAddMod(modInfo).Mod;
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetModDoesNotHaveLanguagesAndDependencyIsNotResolved()
    {
        var baseMod = GameInstallation.InstallAndAddMod("baseMod").Mod;

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = GameInstallation.InstallMod(modInfo).Mod;
        // Do not resolve dependencies here!

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveDefaultLanguageInstalled()
    {
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseModInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseModInstallation.Mod)
            }, DependencyResolveLayout.FullResolved)
        };
        var modInstallation = GameInstallation.InstallAndAddMod(modInfo);
        var mod = modInstallation.Mod;
        mod.ResolveDependencies();

        // Only english is installed
        InstallModLanguage(modInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        // We should not have the languages from base, because only english was detected as physically installed.
        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("en", LanguageSupportLevel.FullLocalized) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveModinfoLanguages()
    {
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseModInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseModInstallation.Mod)
            }, DependencyResolveLayout.FullResolved),
            Languages = new List<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
                new LanguageInfo("de", LanguageSupportLevel.SFX)
            }
        };
        var modInstallation = GameInstallation.InstallAndAddMod(modInfo);
        var mod = modInstallation.Mod;
        mod.ResolveDependencies();

        // Should not be considered
        InstallModLanguage(modInstallation, new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

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
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        InstallModLanguage(baseModInstallation, new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseModInstallation.Mod)
            }, DependencyResolveLayout.FullResolved),
            // Set language
            Languages = new List<ILanguageInfo> { LanguageInfo.Default }
        };
        var modInstallation = GameInstallation.InstallAndAddMod(modInfo);
        var mod = modInstallation.Mod;
        mod.ResolveDependencies();

        // Should not be considered
        InstallModLanguage(modInstallation, new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

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
        var baseMod = GameInstallation.InstallAndAddMod("baseMod").Mod;
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = GameInstallation.InstallAndAddMod(modInfo).Mod;
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesNotHaveLanguagesAndTransitiveDependencyHasLanguages()
    {
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleModInfo = new ModinfoData("middleMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseModInstallation.Mod)
            }, DependencyResolveLayout.FullResolved)
        };
        var middleMod = GameInstallation.InstallAndAddMod(middleModInfo).Mod;
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod)
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = GameInstallation.InstallAndAddMod(modInfo).Mod;
        mod.ResolveDependencies();

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_VirtualMod_TargetModDoesNotHaveLanguagesAndSecondDependencyHasLanguages()
    {
        var baseModInstallation = GameInstallation.InstallAndAddMod("baseMod");
        InstallModLanguage(baseModInstallation, new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleMod = GameInstallation.InstallAndAddMod("middleMod").Mod;
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod), new ModReference(baseModInstallation.Mod)
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
        var mod = GameInstallation.InstallMod(new ModinfoData("myMod")).Mod;
        var langs = _languageFinder.FindLanguages(mod);
        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }
}