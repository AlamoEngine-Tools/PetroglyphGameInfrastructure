using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal sealed class SteamGameClient(IGame game, IServiceProvider serviceProvider) : ClientBase(game, serviceProvider)
{
    public override bool SupportsDebug => true;
    public override bool IsSteamClient => true;
}