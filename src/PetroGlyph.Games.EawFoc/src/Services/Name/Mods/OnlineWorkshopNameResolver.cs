using System;
using System.Globalization;
using EawModinfo.Spec;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name by crawling the name from the mod's steam workshop page.
/// </summary>
public sealed class OnlineWorkshopNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly OfflineWorkshopNameResolver _offlineResolver;
    private readonly ILogger? _logger;

    /// <inheritdoc/>
    public OnlineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _offlineResolver = new OfflineWorkshopNameResolver(serviceProvider);
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(IModReference modReference, CultureInfo culture)
    {
        if (modReference.Type != ModType.Workshops)
            throw new NotSupportedException("Can only resolve for Steam Workshop mods!");
        if (!_steamHelper.ToSteamWorkshopsId(modReference.Identifier, out var modId))
            throw new InvalidOperationException($"Cannot get SteamID from workshops object {modReference.Identifier}");

        try
        {
            var name = _offlineResolver.ResolveCore(modReference, culture);
            return name;
        }
        catch (PetroglyphException)
        {
            _logger?.LogTrace($"Unable to find SteamID '{modId}' in the offline name resolver.");
        }

        var downloader = ServiceProvider.GetService<ISteamWorkshopWebpageDownloader>() ??
                         new SteamWorkshopWebpageDownloader();
        var modsWorkshopWebpage = downloader.GetSteamWorkshopsPageHtmlAsync(modId, culture).GetAwaiter().GetResult();
        if (modsWorkshopWebpage is null)
            throw new InvalidOperationException("Unable to get the mod's workshop web page.");

        return GetName(modsWorkshopWebpage);
    }

    private static string GetName(HtmlDocument htmlDocument)
    {
        if (htmlDocument == null)
            throw new ArgumentNullException(nameof(htmlDocument));

        var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
        if (node is null)
            throw new InvalidOperationException("Unable to get name form Workshop's web page. Mussing 'workshopItemTitle' node.");
        return node.InnerHtml;
    }
}