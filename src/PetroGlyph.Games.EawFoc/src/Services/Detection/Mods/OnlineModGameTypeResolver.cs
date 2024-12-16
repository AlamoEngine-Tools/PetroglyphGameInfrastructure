using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AnakinRaW.CommonUtilities.Collections;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Performs first offline and then online checks to determine a mod's associated game type based on most common Steam Workshop identifiers and available modinfo data.
/// The online checks queries the tags specified by the mod creators. The results are cached for the runtime of the application.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
public sealed class OnlineModGameTypeResolver(IServiceProvider serviceProvider) : ModGameTypeResolver(serviceProvider)
{
    private readonly OfflineModGameTypeResolver _offlineResolver = new(serviceProvider);
    private readonly ISteamGameHelpers _steamGameHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    private readonly ISteamWorkshopWebpageDownloader _steamWebpageDownloader = serviceProvider.GetRequiredService<ISteamWorkshopWebpageDownloader>();

    /// <inheritdoc />
    public override bool TryGetGameType(DetectedModReference modInformation, out ReadOnlyFrugalList<GameType> gameTypes)
    {
        if (_offlineResolver.TryGetGameType(modInformation, out gameTypes))
            return true;
        return modInformation.ModReference.Type == ModType.Workshops && GetGameTypeFromSteamPage(modInformation.Directory.Name, out gameTypes);
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

        return GetGameTypesFromTags(tags, out gameTypes);
    }
}