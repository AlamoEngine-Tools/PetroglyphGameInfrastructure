using System;
using System.Globalization;
using EawModinfo.Spec;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Name;

/// <summary>
/// Resolves a mod's name by crawling the name from the mod's steam workshop page.
/// </summary>
public class OnlineWorkshopNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;

    /// <inheritdoc/>
    public OnlineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(IModReference modReference, CultureInfo culture)
    {
        if (modReference.Type != ModType.Workshops)
            throw new NotSupportedException("Can only resolve for Steam Workshop mods!");
        if (!_steamHelper.ToSteamWorkshopsId(modReference.Identifier, out var modId))
            throw new InvalidOperationException($"Cannot get SteamID from workshops object {modReference.Identifier}");

        var downloader = ServiceProvider.GetService<ISteamWorkshopWebpageDownloader>() ??
                         new SteamWorkshopWebpageDownloader();
        var modsWorkshopWebpage = downloader.GetSteamWorkshopsPageHtmlAsync(modId, culture).GetAwaiter().GetResult();
        if (modsWorkshopWebpage is null)
            throw new InvalidOperationException("Unable to get the mod's workshop web page.");
        return GetName(modsWorkshopWebpage);
    }

    private static string GetName(HtmlDocument htmlDocument)
    {
        Requires.NotNull(htmlDocument, nameof(htmlDocument));
        var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
        if (node is null)
            throw new InvalidOperationException("Unable to get name form Workshop's web page. Mussing 'workshopItemTitle' node.");
        return node.InnerHtml;
    }
}