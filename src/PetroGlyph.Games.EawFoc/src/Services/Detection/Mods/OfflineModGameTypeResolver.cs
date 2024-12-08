using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Collections;
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
    public bool TryGetGameType(IDirectoryInfo modLocation, ModType modType, IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;

        if (modType is ModType.Virtual)
            return false;

        if (HandleWorkshop(modLocation, modType, modinfo, out var gameType))
        {
            gameTypes = new ReadOnlyFrugalList<GameType>(gameType);
            return true;
        }

        var gameCandidateDir = modLocation.Parent?.Parent;
        if (gameCandidateDir is null)
            return false;

        var detector = new DirectoryGameDetector(gameCandidateDir, serviceProvider);

        if (detector.Detect(GameType.Foc, GamePlatform.Undefined).Installed)
        {
            gameTypes = new ReadOnlyFrugalList<GameType>(GameType.Foc);
            return true;
        }

        if (detector.Detect(GameType.Eaw, GamePlatform.Undefined).Installed)
        {
            gameTypes = new ReadOnlyFrugalList<GameType>(GameType.Eaw);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryGetGameType(IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        return GetGameType(modinfo?.SteamData, out gameTypes);
    }

    private bool HandleWorkshop(IDirectoryInfo directory, ModType modType, IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    { 
        gameTypes = default;
        if (modType == ModType.Workshops)
        {
            // Modinfo is superior, even to cache, because, this value can change faster,
            // than new distribution of this library might release.
            // so that the cache would then be invalid.
            if (TryGetGameType(modinfo, out gameTypes))
                return true;

            if (!_steamGameHelpers.ToSteamWorkshopsId(directory.Name, out var steamId))
                return false;

            if (_cache.ContainsMod(steamId))
            {
                gameTypes = new ReadOnlyFrugalList<GameType>(_cache.GetGameTypes(steamId));
                return true;
            }
        }

        return false;
    }

    internal static bool GetGameTypesFromTags(IEnumerable<string> tags, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        var mutableGameTypes = new FrugalList<GameType>();
        
        foreach (var tag in tags)
        {
            var trimmed = tag.AsSpan().Trim();

            if (trimmed.Equals("EAW".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                mutableGameTypes.Add(GameType.Eaw);
            }

            if (trimmed.Equals("FOC".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                mutableGameTypes.Add(GameType.Foc);
            }
        }

        gameTypes = mutableGameTypes.AsReadOnly();
        return gameTypes.Count >= 1;
    }


    private static bool GetGameType(ISteamData? steamData, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;
        return steamData is not null && GetGameTypesFromTags(steamData.Tags, out gameTypes);
    }
}