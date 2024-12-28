using System;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

internal class GameClientFactory(IServiceProvider provider) : IGameClientFactory
{
    public IGameClient CreateClient(IGame game)
    {
        if (game == null) throw 
            new ArgumentNullException(nameof(game));
        if (game.Platform == GamePlatform.SteamGold)
            return new SteamGameClient(game, provider); 
        return new DefaultClient(game, provider);
    }
}