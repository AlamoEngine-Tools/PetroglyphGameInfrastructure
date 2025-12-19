using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBase : TestBaseWithServiceProvider
{
    private ITestingGameInstallation? _gameInstallation;

    protected IFileSystem FileSystem => LazyInitializer.EnsureInitialized(ref field, CreateFileSystem);
    
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

    protected ITestingPhysicalModInstallation InstallAndAddModWithDependencies(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        return GetOrCreateGameInstallation().InstallAndAddMod(name, new DependencyList(deps, layout));
    }
}