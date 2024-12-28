using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;
 
internal sealed class DefaultClient(IGame game, IServiceProvider serviceProvider) : ClientBase(game, serviceProvider)
{
    public override bool SupportsDebug => false;
    public override bool IsSteamClient => false;
}