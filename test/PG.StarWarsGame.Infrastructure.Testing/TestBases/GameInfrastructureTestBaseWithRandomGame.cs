using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

public abstract class GameInfrastructureTestBaseWithRandomGame : GameInfrastructureTestBase
{
    protected IGame Game => GameInstallation.Game;
    
    protected ITestingGameInstallation GameInstallation => GetOrCreateGameInstallation(identity: null);
}