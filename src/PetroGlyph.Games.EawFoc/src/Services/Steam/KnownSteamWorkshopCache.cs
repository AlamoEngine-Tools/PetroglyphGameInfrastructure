using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

internal class KnownSteamWorkshopCache : ISteamWorkshopCache
{
    private static readonly IDictionary<ulong, (string Name, GameType Type)> KnownMods = new Dictionary<ulong, (string, GameType)>
    {
        { 1129810972, ("Republic at War", GameType.Foc) },
        { 1125571106, ("Thrawn's Revenge", GameType.Foc) },
        { 1976399102, ("Fall of the Republic", GameType.Foc) },
        { 1770851727, ("Empire at War: Remake", GameType.Foc) },
        { 1397421866, ("Awakening of the Rebellion", GameType.Foc) },
        { 1126673817, ("The Clone Wars", GameType.Foc) },
        { 1125764259, ("Star Wars Battlefront Commander", GameType.Foc) },
        { 1130150761, ("Old Republic at War", GameType.Foc) },
        { 1382582782, ("Absolute Chaos", GameType.Foc) },
        { 1780988753, ("Rise of the Mandalorians", GameType.Foc) },
        { 1126880602, ("Stargate - Empire at War: Pegasus Chronicles", GameType.Foc) },
        { 1235783994, ("Phoenix Rising", GameType.Foc) },
        { 1241979729, ("Star Wars Alliance Rebellion", GameType.Foc) },
    };

    public bool ContainsMod(ulong id)
    {
        return KnownMods.ContainsKey(id);
    }

    public string GetName(ulong id)
    {
        return KnownMods[id].Name;
    }

    public GameType GetGameType(ulong id)
    {
        return KnownMods[id].Type;
    }
}