using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

/// <summary>
/// Serves as a specialized base class for testing infrastructure components of the Petroglyph Star Wars game test framework 
/// with a randomly initialized game instance.
/// </summary>
public abstract class GameInfrastructureTestBaseWithRandomGame : GameInfrastructureTestBase
{
    /// <summary>
    /// Gets the instance of the game associated with the current test context.
    /// </summary>
    protected IGame Game => GameInstallation.Game;

    /// <summary>
    /// Gets the instance of the <see cref="ITestingGameInstallation"/> associated with the current test context.
    /// </summary>
    protected ITestingGameInstallation GameInstallation => GetOrCreateGameInstallation(identity: null);
}