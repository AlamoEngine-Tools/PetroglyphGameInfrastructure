using System;
using System.Globalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name from a cached list of known Steam workshop IDs.
/// </summary>
public sealed class OfflineWorkshopNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly ISteamWorkshopCache _workshopCache;

    /// <inheritdoc/>
    public OfflineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _workshopCache = serviceProvider.GetRequiredService<ISteamWorkshopCache>();
    }

    /// <inheritdoc/>
    protected internal override string? ResolveCore(IModReference modReference, CultureInfo culture)
    {
        if (modReference.Type != ModType.Workshops)
            throw new NotSupportedException("Can only resolve for Steam Workshop mods!");
        if (!_steamHelper.ToSteamWorkshopsId(modReference.Identifier, out var modId))
            throw new ModException(modReference, $"Cannot get SteamID from workshops object {modReference.Identifier}");

        return _workshopCache.ContainsMod(modId) ? _workshopCache.GetName(modId) : null;
    }
}