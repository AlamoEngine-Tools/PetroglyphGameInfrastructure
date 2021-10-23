using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Language;

namespace PetroGlyph.Games.EawFoc.Clients;

public sealed class SteamGameLanguageFinder : IGameLanguageFinder
{
    private readonly ISteamWrapper _steamWrapper;

    private readonly Dictionary<uint, string> _localizationDepots = new()
    {
        { 32473 , "fr"},
        { 32474 , "de"},
        { 32475 , "it"},
        { 32476 , "es"},
    };

    public SteamGameLanguageFinder(IServiceProvider serviceProvider)
    {
        _steamWrapper = serviceProvider.GetRequiredService<ISteamWrapper>();
    }

    public ISet<ILanguageInfo> FindInstalledLanguages(IGame game)
    {
        if (game.Platform != GamePlatform.SteamGold)
            throw new InvalidOperationException("This service only is supported by Steam games.");
        if (!_steamWrapper.IsGameInstalled(32470u, out var manifest))
            throw new InvalidOperationException("Empire at War is not registered as a Steam Game");

        var result = new HashSet<ILanguageInfo>();
        foreach (var depot in manifest!.Depots)
        {
            if (_localizationDepots.TryGetValue(depot, out var languageCode))
                result.Add(new LanguageInfo(languageCode, LanguageSupportLevel.FullLocalized));
        }
        return result;
    }
}