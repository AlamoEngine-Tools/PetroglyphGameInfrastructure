using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class CommonTestBaseWithRandomGame : CommonTestBase
{
    protected readonly IGame Game;

    protected CommonTestBaseWithRandomGame()
    {
        Game = CreateRandomGame();
    }

    protected IMod CreateAndAddMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        return CreateAndAddMod(Game, GITestUtilities.GetRandomWorkshopFlag(Game), name, new DependencyList(deps, layout));
    }

    protected IMod CreateAndAddMod(IModinfo modinfo)
    {
        return CreateAndAddMod(Game, GITestUtilities.GetRandomWorkshopFlag(Game), modinfo);
    }
}