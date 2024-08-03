using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

internal interface ISteamWorkshopCache
{
    bool ContainsMod(ulong id);

    string GetName(ulong id);

    GameType GetGameType(ulong id);
}