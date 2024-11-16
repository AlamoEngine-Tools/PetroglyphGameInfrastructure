using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class LanguageFinderTest : CommonTestBase
{
    private readonly InstalledLanguageFinder _languageFinder;

    public LanguageFinderTest()
    {
        _languageFinder = new InstalledLanguageFinder(ServiceProvider);
    }

    [Fact]
    public void NullArgTest_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _languageFinder.FindLanguages(null!));
    }

    [Fact]
    public void FindLanguages_Game_WithNoLanguages()
    {
        var game = CreateRandomGame();
        var langs = _languageFinder.FindLanguages(game);
        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Game_WithLanguages_En_De()
    {
        var game = CreateRandomGame();
        game.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        game.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));
        var langs = _languageFinder.FindLanguages(game);

        Assert.Equivalent(new List<ILanguageInfo>
        {
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
        }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_WithNoLanguages_UsesGame()
    {
        var game = CreateRandomGame();
        game.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var mod = game.InstallMod("myMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>{ new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_WithLanguages_En_De()
    {
        var game = FileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
        var mod = game.InstallMod("myMod", true, ServiceProvider);
        mod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        mod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

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
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        baseMod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions{ResolveCompleteChain = true});

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
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions { ResolveCompleteChain = true });

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_InheritLanguage_TargetModDoesNotHaveLanguagesAndDependencyIsNotResolved()
    {
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        // Do not resolve dependencies here!

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveDefaultLanguageInstalled()
    {
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        baseMod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions { ResolveCompleteChain = true });

        // Only english is installed
        mod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var langs = _languageFinder.FindLanguages(mod);

        // We should not have the languages from base, because only english was detected as physically installed.
        Assert.Equivalent(new List<ILanguageInfo>{new LanguageInfo("en", LanguageSupportLevel.FullLocalized)}, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesHaveModinfoLanguages()
    {
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        baseMod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

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
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions { ResolveCompleteChain = true });

        // Should not be considered
        mod.InstallLanguage(new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

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
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        baseMod.InstallLanguage(new LanguageInfo("en", LanguageSupportLevel.FullLocalized));

        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved),
            // Set language
            Languages = new List<ILanguageInfo> { LanguageInfo.Default }
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions());

        // Should not be considered
        mod.InstallLanguage(new LanguageInfo("it", LanguageSupportLevel.FullLocalized));

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
        var game = CreateRandomGame();
        game.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions());

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>{ new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_Mod_TargetModDoesNotHaveLanguagesAndSecondDependencyHasLanguages()
    {
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleModInfo = new ModinfoData("middleMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(baseMod)
            }, DependencyResolveLayout.FullResolved)
        };
        var middleMod = game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), middleModInfo, ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod)
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), modInfo, ServiceProvider);
        mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions{ResolveCompleteChain = true});

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo>{ new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }

    [Fact]
    public void FindLanguages_VirtualMod_TargetModDoesNotHaveLanguagesAndSecondDependencyHasLanguages()
    {
        var game = CreateRandomGame();
        var baseMod = game.InstallAndAddMod("baseMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        baseMod.InstallLanguage(new LanguageInfo("de", LanguageSupportLevel.SFX));
        var middleMod = game.InstallAndAddMod("middleMod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        var modInfo = new ModinfoData("myMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(middleMod), new ModReference(baseMod)
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = new VirtualMod(game, modInfo, ServiceProvider);

        var langs = _languageFinder.FindLanguages(mod);

        Assert.Equivalent(new List<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, langs, true);
    }


    [Fact]
    public void FindLanguages_Mod_FromModinfo_WithNoLanguages()
    {
        var game = CreateRandomGame();
        var mod = game.InstallMod(GITestUtilities.GetRandomWorkshopFlag(game), new ModinfoData("myMod"), ServiceProvider);
        var langs = _languageFinder.FindLanguages(mod);
        Assert.Equivalent(new List<ILanguageInfo>(), langs, true);
    }
}