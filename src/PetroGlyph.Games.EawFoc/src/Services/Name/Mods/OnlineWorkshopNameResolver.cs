using System;
using System.Collections.Concurrent;
using System.Globalization;
using EawModinfo.Spec;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name by crawling the name from the mod's steam workshop page.
/// </summary>
public sealed class OnlineWorkshopNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;

    private readonly ConcurrentDictionary<ulong, string?> _nameCache;

    /// <inheritdoc/>
    public OnlineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _nameCache = new ConcurrentDictionary<ulong, string?>();
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(IModReference modReference, CultureInfo culture)
    {
        if (modReference.Type != ModType.Workshops)
            throw new NotSupportedException("Can only resolve for Steam Workshop mods!");

        if (!_steamHelper.ToSteamWorkshopsId(modReference.Identifier, out var modId))
            throw new ModException(modReference, $"Cannot get SteamID from workshops object {modReference.Identifier}");

        return _nameCache.GetOrAdd(modId, id =>
        {
            if (_nameCache.TryGetValue(modId, out var name))
                return name!;

            var downloader = ServiceProvider.GetService<ISteamWorkshopWebpageDownloader>() ??
                             new SteamWorkshopWebpageDownloader();
            var modsWorkshopWebpage = downloader.GetSteamWorkshopsPageHtmlAsync(modId, culture).GetAwaiter().GetResult();
            if (modsWorkshopWebpage is null)
                throw new InvalidOperationException("Unable to get the mod's workshop web page.");

            return GetName(modsWorkshopWebpage);
        })!;
    }

    private static string GetName(HtmlDocument htmlDocument)
    {
        if (htmlDocument == null)
            throw new ArgumentNullException(nameof(htmlDocument));

        var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
        if (node is null)
            throw new InvalidOperationException("Unable to get name form Workshop's web page. Missing 'workshopItemTitle' node.");
        return node.InnerHtml;
    }
}