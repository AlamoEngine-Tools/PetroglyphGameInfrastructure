using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBaseWithRandomGame : GameInfrastructureTestBase
{
    protected IGame Game => GameInstallation.Game;
    
    protected ITestingGameInstallation GameInstallation => GetOrCreateGameInstallation(identity: null);

    protected ITestingModInstallation CreateAndAddMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        return CreateAndAddModInstallation(GITestUtilities.GetRandomWorkshopFlag(Game), name, new DependencyList(deps, layout));
    }

    protected ITestingModInstallation CreateAndAddMod(IModinfo modinfo)
    {
        return CreateAndAddModInstallation(GITestUtilities.GetRandomWorkshopFlag(Game), modinfo);
    }
}