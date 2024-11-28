using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing;

public abstract class CommonTestBaseWithRandomGame : CommonTestBase
{
    protected readonly IGame Game;

    protected CommonTestBaseWithRandomGame()
    {
        Game = CreateRandomGame();
    }

    protected IMod CreateAndAddMod(string name, DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, params IModReference[] deps)
    {
        return CreateAndAddMod(Game, name, layout, deps);
    }
}