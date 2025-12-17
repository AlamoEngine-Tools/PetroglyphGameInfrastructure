using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBaseWithRandomGame : GameInfrastructureTestBase
{
    protected readonly IGame Game;

    protected GameInfrastructureTestBaseWithRandomGame()
    { 
        InstallRandomGame();
        Game = GameInstallation.Game;
    }

    protected IMod CreateAndAddMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        return CreateAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), name, new DependencyList(deps, layout));
    }

    protected IMod CreateAndAddMod(IModinfo modinfo)
    {
        return CreateAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modinfo);
    }
}