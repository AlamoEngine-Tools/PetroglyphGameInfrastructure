using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;
using EawModinfo.Spec;
using EawModinfo.Spec.Steam;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Base class for an <see cref="IModGameTypeResolver"/>.
/// </summary>
public abstract class ModGameTypeResolver(IServiceProvider serviceProvider) : IModGameTypeResolver
{
    /// <summary>
    /// The service provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider = serviceProvider;

    /// <inheritdoc />
    public abstract bool TryGetGameType(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes);

    /// <inheritdoc />
    public virtual bool TryGetGameType(IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        return GetGameType(modinfo?.SteamData, out gameTypes);
    }

    /// <inheritdoc />
    public bool IsDefinitelyNotCompatibleToGame(DetectedModReference modInformation, GameType expected)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return TryGetGameType(modInformation, out var gameTypes) && !gameTypes.Contains(expected);
    }

    /// <inheritdoc />
    public bool IsDefinitelyNotCompatibleToGame(IModinfo? modinfo, GameType expectedGameType)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return TryGetGameType(modinfo, out var gameTypes) && !gameTypes.Contains(expectedGameType);
    }

    private static bool GetGameType(ISteamData? steamData, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;
        return steamData is not null && GetGameTypesFromTags(steamData.Tags, out gameTypes);
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
}

/// <summary>
/// Performs offline checks to determine a mod's associated game type based on most common Steam Workshop identifiers and available modinfo data.
/// </summary>
/// <remarks>
/// The class will not do any file system-based heuristics as the false-negative rate might be too high.
/// </remarks>
/// <param name="serviceProvider">The service provider.</param>
public sealed class OfflineModGameTypeResolver(IServiceProvider serviceProvider) : ModGameTypeResolver(serviceProvider)
{
    private readonly ISteamWorkshopCache _cache = serviceProvider.GetRequiredService<ISteamWorkshopCache>();
    private readonly ISteamGameHelpers _steamGameHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();

    /// <inheritdoc />
    public override bool TryGetGameType(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        if (modInformation == null) 
            throw new ArgumentNullException(nameof(modInformation));

        gameTypes = default;

        if (modInformation.ModReference.Type is ModType.Virtual)
            return false;

        if (HandleWorkshop(modInformation, out var gameType))
        {
            gameTypes = new ReadOnlyFrugalList<GameType>(gameType);
            return true;
        }

        var gameCandidateDir = modInformation.Directory.Parent?.Parent;
        if (gameCandidateDir is null)
            return false;

        var detector = new DirectoryGameDetector(gameCandidateDir, ServiceProvider);

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

    private bool HandleWorkshop(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes)
    { 
        gameTypes = default;
        if (modInformation.ModReference.Type == ModType.Workshops)
        {
            // Modinfo is superior, even to cache, because, this value can change faster,
            // than new distribution of this library might release.
            // so that the cache would then be invalid.
            if (TryGetGameType(modInformation.Modinfo, out gameTypes))
                return true;

            if (!_steamGameHelpers.ToSteamWorkshopsId(modInformation.Directory.Name, out var steamId))
                return false;

            if (_cache.ContainsMod(steamId))
            {
                gameTypes = new ReadOnlyFrugalList<GameType>(_cache.GetGameTypes(steamId));
                return true;
            }
        }

        return false;
    }
}