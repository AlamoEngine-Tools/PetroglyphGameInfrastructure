using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
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
    public bool TryGetGameType(IDirectoryInfo modLocation, ModType modType, IModinfo? modinfo, out GameType gameType)
    {
        if (_offlineResolver.TryGetGameType(modLocation, modType, modinfo, out gameType))
            return true;

        if (modType != ModType.Workshops)
            return false;
        return GetGameTypeFromSteamPage(modLocation.Name, out gameType);
    }

    /// <inheritdoc />
    public bool TryGetGameType(IModinfo? modinfo, out GameType gameType)
    {
        if (_offlineResolver.TryGetGameType(modinfo, out gameType))
            return true;
        if (modinfo?.SteamData is null)
            return false;
        return GetGameTypeFromSteamPage(modinfo.SteamData.Id, out gameType);
    }

    private bool GetGameTypeFromSteamPage(string steamIdValue, out GameType gameType)
    {
        gameType = default;
        if (!_steamGameHelpers.ToSteamWorkshopsId(steamIdValue, out var steamId))
            return false;
        
        var nullableGameType = _gameTypeCache.GetOrAdd(steamId, id =>
        {
            var webPage = _steamWebpageDownloader.GetSteamWorkshopsPageHtmlAsync(steamId, CultureInfo.InvariantCulture)
                .GetAwaiter().GetResult();
            if (webPage is null)
                return null;
            var tagNodes = webPage.DocumentNode.SelectNodes("//div[@class='workshopTags']/a/text()");
            var tags = new HashSet<string>(tagNodes.Select(x => x.InnerHtml.ToUpperInvariant().Trim()));
            if (tags.Contains("FOC"))
                return GameType.Foc;
            if (tags.Contains("EAW"))
                return GameType.Eaw;
            return null;
        });

        if (nullableGameType.HasValue)
        {
            gameType = nullableGameType.Value;
            return true;
        }

        return false;
    }
}