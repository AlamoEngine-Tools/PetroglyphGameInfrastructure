using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using EawModinfo.Spec.Steam;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Performs offline checks to determine a mod's associated game type based on most common Steam Workshop identifiers and available modinfo data.
/// </summary>
/// <remarks>
/// The class will not do any file system-based heuristics as the false-negative rate might be too high.
/// </remarks>
/// <param name="serviceProvider">The service provider.</param>
public sealed class OfflineModGameTypeResolver(IServiceProvider serviceProvider) : IModGameTypeResolver
{
    private readonly ISteamWorkshopCache _cache = serviceProvider.GetRequiredService<ISteamWorkshopCache>();
    private readonly ISteamGameHelpers _steamGameHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();

    /// <inheritdoc />
    public bool TryGetGameType(IDirectoryInfo modLocation, ModType modType, IModinfo? modinfo, out GameType gameType)
    {
        gameType = default;

        if (modType == ModType.Virtual)
            return false;

        if (HandleWorkshop(modLocation, modType, out gameType))
            return true;

        if (TryGetGameType(modinfo, out gameType))
            return true;

        // TODO: If mod is in game\mods\modDir return gametype of the game.
        return false;
    }

    /// <inheritdoc />
    public bool TryGetGameType(IModinfo? modinfo, out GameType gameType)
    {
        return GetGameType(modinfo?.SteamData, out gameType);
    }

    private bool HandleWorkshop(IDirectoryInfo directory, ModType modType, out GameType gameType)
    {
        gameType = default;
        if (modType == ModType.Workshops)
        {
            if (!_steamGameHelpers.ToSteamWorkshopsId(directory.Name, out var steamId))
                return false;

            if (_cache.ContainsMod(steamId))
            {
                gameType = _cache.GetGameType(steamId);
                return true;
            }
        }

        return false;
    }
    
    private static bool GetGameType(ISteamData? steamData, out GameType gameType)
    {
        gameType = GameType.Eaw;
        if (steamData is null)
            return false;
        var tags = new HashSet<string>(steamData.Tags.Select(x => x.Trim().ToUpperInvariant()));
        if (tags.Contains("FOC"))
        {
            gameType = GameType.Foc;
            return true;
        }
        if (tags.Contains("EAW"))
        {
            gameType = GameType.Eaw;
            return true;
        }
        return false;
    }
}