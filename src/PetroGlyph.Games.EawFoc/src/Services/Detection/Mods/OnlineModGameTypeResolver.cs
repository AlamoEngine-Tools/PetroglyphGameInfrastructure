using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.Collections;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Performs first offline and then online checks to determine a mod's associated game type based on most common Steam Workshop identifiers and available modinfo data.
/// The online checks queries the tags specified by the mod creators. The results are cached for the runtime of the application.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
public sealed class OnlineModGameTypeResolver(IServiceProvider serviceProvider) : IModGameTypeResolver
{
    private readonly OfflineModGameTypeResolver _offlineResolver = new(serviceProvider);
    private readonly ISteamGameHelpers _steamGameHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    private readonly ISteamWorkshopWebpageDownloader _steamWebpageDownloader = serviceProvider.GetRequiredService<ISteamWorkshopWebpageDownloader>();

    private readonly ConcurrentDictionary<ulong, GameType?> _gameTypeCache = new();

    /// <inheritdoc />
    public bool TryGetGameType(IDirectoryInfo modLocation, ModType modType, IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        if (_offlineResolver.TryGetGameType(modLocation, modType, modinfo, out gameTypes))
            return true;

        if (modType != ModType.Workshops)
            return false;
        return GetGameTypeFromSteamPage(modLocation.Name, out gameTypes);
    }

    /// <inheritdoc />
    public bool TryGetGameType(IModinfo? modinfo, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;
        return _offlineResolver.TryGetGameType(modinfo, out gameTypes);
    }

    private bool GetGameTypeFromSteamPage(string steamIdValue, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        gameTypes = default;
        if (!_steamGameHelpers.ToSteamWorkshopsId(steamIdValue, out var steamId))
            return false;
        var webPage = _steamWebpageDownloader.GetSteamWorkshopsPageHtmlAsync(steamId, CultureInfo.InvariantCulture)
            .GetAwaiter().GetResult();
        if (webPage is null)
            return false;

        var tagNodes = webPage.DocumentNode.SelectNodes("//div[@class='workshopTags']/a/text()");
        if (tagNodes is null || tagNodes.Count == 0)
            return false;

        var tags = new HashSet<string>(tagNodes.Select(x => x.InnerHtml));

        return OfflineModGameTypeResolver.GetGameTypesFromTags(tags, out gameTypes);
    }
}