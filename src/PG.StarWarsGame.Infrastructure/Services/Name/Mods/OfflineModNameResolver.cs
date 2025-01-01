using System;
using System.Globalization;
using System.IO.Abstractions;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name from a cached list of known Steam workshop IDs.
/// </summary>
public sealed class OfflineModNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly ISteamWorkshopCache _workshopCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineModNameResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public OfflineModNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _workshopCache = serviceProvider.GetRequiredService<ISteamWorkshopCache>();
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(DetectedModReference detectedMod, CultureInfo culture)
    {
        if (detectedMod.ModReference.Type == ModType.Workshops && 
            _steamHelper.ToSteamWorkshopsId(detectedMod.Directory.Name, out var modId))
            return GetWorkshopName(modId);

        return GetNameFromDirectory(detectedMod.Directory);
    }

    private string GetWorkshopName(ulong steamId)
    {
        return _workshopCache.ContainsMod(steamId) ? _workshopCache.GetName(steamId) : steamId.ToString();
    }

    private static string GetNameFromDirectory(IDirectoryInfo directory)
    {
        var removedUnderscore = directory.Name.Replace('_', ' ');
        var removedDash = removedUnderscore.Replace('-', ' ');
        var trimmed = removedDash.Trim();
        return string.IsNullOrEmpty(trimmed) ? directory.Name : trimmed;
    }
}