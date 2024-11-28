﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing;

public abstract class CommonTestBase
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly MockFileSystem FileSystem = new();

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected CommonTestBase()
    {
        var sc = new ServiceCollection();
        SetupServiceProvider(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    protected virtual void SetupServiceProvider(IServiceCollection sc)
    {
        sc.AddSingleton<IFileSystem>(FileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
    }

    public static IEnumerable<object[]> RealGameIdentities()
    {
        foreach (var platform in GITestUtilities.RealPlatforms)
        {
            yield return [new GameIdentity(GameType.Eaw, platform)];
            yield return [new GameIdentity(GameType.Foc, platform)];
        }
    }

    protected static GameIdentity CreateRandomGameIdentity()
    {
        return new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), TestHelpers.GetRandom(GITestUtilities.RealPlatforms));
    }

    protected PetroglyphStarWarsGame CreateRandomGame()
    {
        return FileSystem.InstallGame(CreateRandomGameIdentity(), ServiceProvider);
    }

    private static readonly string[] PossibleLanguages = ["en", "de", "es", "it"]; 

    protected static ICollection<ILanguageInfo> GetRandomLanguages()
    {
        var languages = new HashSet<ILanguageInfo>(PossibleLanguages.Length);

        for (var i = 0; i < PossibleLanguages.Length; i++)
        {
            var code = TestHelpers.GetRandom(PossibleLanguages);
            var support = TestHelpers.GetRandomEnum<LanguageSupportLevel>();
            languages.Add(new LanguageInfo(code, support));
        }

        return languages;
    }

    protected IMod CreateAndAddMod(IGame game, string name, DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, params IList<IModReference> deps)
    {
        if (deps.Count == 0)
            return game.InstallAndAddMod(name, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(deps, layout)
        };

        return game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(game), modinfo, ServiceProvider);
    }
}