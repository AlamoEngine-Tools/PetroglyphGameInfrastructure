using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal sealed class SteamGameClientsFactory(IServiceProvider serviceProvider) : IGameClientFactory
{
    public IGameClient CreateClient(IGame game)
    {
        return game.Platform is GamePlatform.SteamGold
            ? new SteamPetroglyphStarWarsGameClient(game, serviceProvider)
            : new PetroglyphStarWarsGameClient(game, serviceProvider);
    }
}