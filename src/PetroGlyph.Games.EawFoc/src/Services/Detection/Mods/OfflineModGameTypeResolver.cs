using System;
using AnakinRaW.CommonUtilities.Collections;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
public sealed class OfflineModGameTypeResolver(IServiceProvider serviceProvider) : ModGameTypeResolver(serviceProvider)
{
    private readonly ISteamWorkshopCache _cache = serviceProvider.GetRequiredService<ISteamWorkshopCache>();
    private readonly ISteamGameHelpers _steamGameHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();


    /// <inheritdoc />
    protected internal override bool TryGetGameTypeCore(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;

        if (modInformation.ModReference.Type is ModType.Virtual)
            return false;

        if (HandleWorkshop(modInformation, out gameTypes))
            return true;

        Logger?.LogTrace("Checking whether mod location is inside a game's Mods directory.");

        if (!"Mods".Equals(modInformation.Directory.Parent?.Name, StringComparison.OrdinalIgnoreCase))
            return false;

        var gameCandidateDir = modInformation.Directory.Parent?.Parent;
        if (gameCandidateDir is null)
            return false;

        var detector = new DirectoryGameDetector(gameCandidateDir, ServiceProvider);

        if (detector.Detect(GameType.Foc, GamePlatform.Undefined).Installed)
        {
            Logger?.LogTrace($"{modInformation.ModReference} is located in FoC's Mods directory.");
            gameTypes = new ReadOnlyFrugalList<GameType>(GameType.Foc);
            return true;
        }

        if (detector.Detect(GameType.Eaw, GamePlatform.Undefined).Installed)
        {
            Logger?.LogTrace($"{modInformation.ModReference} is located in EaW's Mods directory.");
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
            if (!_steamGameHelpers.ToSteamWorkshopsId(modInformation.Directory.Name, out var steamId))
                return false;

            // Modinfo is superior to cache, because this value can change faster,
            // than new distribution of this library might release.
            // so that the cache would then be invalid.
            if (TryGetGameType(modInformation.Modinfo, out gameTypes))
                return true;

            if (_cache.ContainsMod(steamId))
            {
                gameTypes = new ReadOnlyFrugalList<GameType>(_cache.GetGameTypes(steamId));
                return true;
            }
        }

        return false;
    }
}