using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBase
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly MockFileSystem FileSystem = new();

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected GameInfrastructureTestBase()
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

    protected static GameIdentity CreateRandomGameIdentity()
    {
        return new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), TestHelpers.GetRandom(GITestUtilities.RealPlatforms));
    }

    protected PetroglyphStarWarsGame CreateRandomGame()
    {
        return FileSystem.InstallGame(CreateRandomGameIdentity(), ServiceProvider);
    }

    protected IMod CreateAndAddMod(IGame game, bool isWorkshop, string name, IModDependencyList dependencies)
    {
        if (dependencies.Count == 0)
            return game.InstallAndAddMod(name, isWorkshop, ServiceProvider);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = dependencies
        };
        return CreateAndAddMod(game, isWorkshop, modinfo);
    }

    protected IMod CreateAndAddMod(IGame game, bool isWorkshop, IModinfo modinfo)
    {
        return game.InstallAndAddMod(isWorkshop, modinfo, ServiceProvider);
    }
}