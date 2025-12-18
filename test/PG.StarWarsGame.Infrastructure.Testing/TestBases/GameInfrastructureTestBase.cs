using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBase : TestBaseWithServiceProvider
{
    protected IFileSystem FileSystem => LazyInitializer.EnsureInitialized(ref field, CreateFileSystem);

    protected ITestingGameInstallation GameInstallation { get; }

    protected GameInfrastructureTestBase()
    {
        GameInstallation = GameInfrastructureTesting.Game(ServiceProvider);
    }
    
    protected virtual IFileSystem CreateFileSystem()
    {
        return new MockFileSystem();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(FileSystem);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
    }

    protected virtual IGame GetOrÍnstallGame(IGameIdentity? identity = null)
    {
        if (GameInstallation.Game is not null)
            return GameInstallation.Game;
        return identity is not null ? GameInstallation.Install(identity) : GameInstallation.InstallRandom();
    }


    // TODO: To installation
    protected IMod CreateAndAddMod(bool isWorkshop, string name, IModDependencyList dependencies)
    {
        if (dependencies.Count == 0)
            return GameInstallation.Game.InstallAndAddMod(name, isWorkshop, ServiceProvider);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = dependencies
        };
        return CreateAndAddMod(isWorkshop, modinfo);
    }

    protected IMod CreateAndAddMod(bool isWorkshop, IModinfo modinfo)
    {
        return GameInstallation.Game.InstallAndAddMod(isWorkshop, modinfo, ServiceProvider);
    }
}