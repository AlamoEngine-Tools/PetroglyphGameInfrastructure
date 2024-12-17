using System;
using System.Collections.Generic;
using AET.SteamAbstraction;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Language;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Searches installed game languages, based on the Steam Game Manifest.
/// </summary>
internal sealed class SteamGameLanguageFinder : InstalledLanguageFinder
{
    private readonly ISteamWrapper _steamWrapper;
    private readonly Dictionary<uint, string> _localizationDepots = new()
    {
        { 32473 , "fr"},
        { 32474 , "de"},
        { 32475 , "it"},
        { 32476 , "es"},
    };

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SteamGameLanguageFinder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        _steamWrapper = serviceProvider.GetRequiredService<ISteamWrapper>();
    }

    protected override IReadOnlyCollection<ILanguageInfo> GetInstalledGameLanguages(IGame game)
    {
        if (game.Platform != GamePlatform.SteamGold || !_steamWrapper.IsGameInstalled(32470u, out var manifest))
            return base.GetInstalledGameLanguages(game);

        // English is always included by default.
        var result = new HashSet<ILanguageInfo> { new LanguageInfo("en", LanguageSupportLevel.FullLocalized) };
        foreach (var depot in manifest.Depots)
        {
            if (_localizationDepots.TryGetValue(depot, out var languageCode))
                result.Add(new LanguageInfo(languageCode, LanguageSupportLevel.FullLocalized));
        }
        return result;
    }
}