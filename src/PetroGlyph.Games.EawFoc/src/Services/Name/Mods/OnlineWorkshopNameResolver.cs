using System;
using System.Collections.Concurrent;
using System.Globalization;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name by crawling the name from the mod's steam workshop page.
/// </summary>
public sealed class OnlineModNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly ConcurrentDictionary<ulong, string> _nameCache;
    private readonly OfflineModNameResolver _offlineModNameResolver;
    private readonly ISteamWorkshopWebpageDownloader _steamWebpageDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnlineModNameResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public OnlineModNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _nameCache = new ConcurrentDictionary<ulong, string>();
        _offlineModNameResolver = new OfflineModNameResolver(serviceProvider);
        _steamWebpageDownloader = serviceProvider.GetRequiredService<ISteamWorkshopWebpageDownloader>();
        foreach (var mod in KnownSteamWorkshopCache.KnownMods) 
            _nameCache.TryAdd(mod.Key, mod.Value.Name);
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(DetectedModReference detectedMod, CultureInfo culture)
    {
        if (detectedMod.ModReference.Type != ModType.Workshops || !_steamHelper.ToSteamWorkshopsId(detectedMod.Directory.Name, out var modId))
            return _offlineModNameResolver.ResolveCore(detectedMod, culture);

        return _nameCache.GetOrAdd(modId, id =>
        {
            var name = GetNameFromOnline(id, culture);
            if (string.IsNullOrEmpty(name))
                name = _offlineModNameResolver.ResolveCore(detectedMod, culture);
            return name!;
        })!;
    }

    private string? GetNameFromOnline(ulong modId, CultureInfo culture)
    {
        var modsWorkshopWebpage = _steamWebpageDownloader.GetSteamWorkshopsPageHtmlAsync(modId, culture)
            .GetAwaiter().GetResult();

        if (modsWorkshopWebpage == null)
        {
            Logger?.LogTrace($"Unable to download website for Steam ID '{modId}'.");
            return null;
        }

        var node = modsWorkshopWebpage.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
        if (node is null)
        {
            Logger?.LogTrace($"Unable to find the item title on website for Steam ID '{modId}',");
            return null;
        }
        return node.InnerHtml;
    }
}