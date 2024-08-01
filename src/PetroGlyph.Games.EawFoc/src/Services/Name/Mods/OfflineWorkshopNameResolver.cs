using System;
using System.Collections.Generic;
using System.Globalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name from a cached list of known Steam workshop IDs.
/// </summary>
public sealed class OfflineWorkshopNameResolver : ModNameResolverBase
{
    private readonly ISteamGameHelpers _steamHelper;

    private static readonly IDictionary<ulong, string> KnownModIds = new Dictionary<ulong, string>
    {
        {1129810972, "Republic at War"},
        {1125571106, "Thrawn's Revenge"},
        {1976399102, "Fall of the Republic"},
        {1770851727, "Empire at War: Remake"},
        {1397421866, "Awakening of the Rebellion"},
        {1126673817, "The Clone Wars"},
        {1125764259, "Star Wars Battlefront Commander"},
        {1130150761, "Old Republic at War"},
        {1382582782, "Absolute Chaos"},
        {1780988753, "Rise of the Mandalorians"},
        {1126880602, "Stargate - Empire at War: Pegasus Chronicles"},
        {1235783994, "Phoenix Rising"},
        {1241979729, "Star Wars Alliance Rebellion"},
    };

    /// <inheritdoc/>
    public OfflineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
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

        KnownModIds.TryGetValue(modId, out var name);
        return name!;
    }
}