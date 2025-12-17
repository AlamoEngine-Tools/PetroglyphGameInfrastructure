using System;
using System.IO.Abstractions;
using System.Threading;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Testing;
using AET.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBase : TestBaseWithServiceProvider
{
    public IFileSystem FileSystem => LazyInitializer.EnsureInitialized(ref field, CreateFileSystem);
    
    protected virtual IFileSystem CreateFileSystem()
    {
        return new MockFileSystem();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(FileSystem);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
    }

    protected static GameIdentity CreateRandomGameIdentity()
    {
        return new GameIdentity(Random.Enum<GameType>(), Random.Item(GITestUtilities.RealPlatforms));
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