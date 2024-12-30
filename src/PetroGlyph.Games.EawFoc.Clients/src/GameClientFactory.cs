using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

internal sealed class GameClientFactory(IServiceProvider serviceProvider) : IGameClientFactory
{
    public IGameClient CreateClient(IGame game)
    {
        if (game == null) throw 
            new ArgumentNullException(nameof(game));
        return new PetroglyphStarWarsGameClient(game, serviceProvider);
    }
}