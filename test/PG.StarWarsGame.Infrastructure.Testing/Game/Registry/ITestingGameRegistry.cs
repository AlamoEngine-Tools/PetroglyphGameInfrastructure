using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Registry;

public interface ITestingGameRegistry
{
    IGameRegistry CreateNonExistingRegistry(GameType gameType);

    IGameRegistry CreateInstlled(IGame game);

    IGameRegistry CreateFrom(TestGameRegistrySetupData registrySetupData);
}