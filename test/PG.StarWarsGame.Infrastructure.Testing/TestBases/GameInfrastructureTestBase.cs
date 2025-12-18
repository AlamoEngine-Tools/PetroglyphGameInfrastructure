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

    private ITestingGameInstallation? _gameInstallation;

    
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

    protected virtual ITestingGameInstallation GetOrCreateGameInstallation(IGameIdentity? identity = null)
    { 
        if (_gameInstallation is not null)
            return _gameInstallation;
        identity ??= GITestUtilities.GetRandomGameIdentity(realOnly: true);
        return _gameInstallation = GameInfrastructureTesting.Game(identity, ServiceProvider);
    }


    // TODO: To installation
    protected IMod CreateAndAddMod(bool isWorkshop, string name, IModDependencyList dependencies)
    {
        if (dependencies.Count == 0)
            return GetOrCreateGameInstallation().Game.InstallAndAddMod(name, isWorkshop, ServiceProvider);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = dependencies
        };
        return CreateAndAddMod(isWorkshop, modinfo);
    }

    protected IMod CreateAndAddMod(bool isWorkshop, IModinfo modinfo)
    {
        return GetOrCreateGameInstallation().Game.InstallAndAddMod(isWorkshop, modinfo, ServiceProvider);
    }
}