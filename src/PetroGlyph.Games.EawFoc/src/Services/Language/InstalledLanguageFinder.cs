using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service that searches installed localizations for playable objects.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
public class InstalledLanguageFinder(IServiceProvider serviceProvider) : ILanguageFinder
{
    /// <summary>
    /// The service provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Searches for installed languages for the specified playable object. When no languages are found the method returns an empty collection.
    /// </summary>
    /// <remarks>
    /// For mods: If a mod has modinfo data and this data has explicitly set languages, the value from modinfo is returned as is.
    /// Also, in the case a mod does not have any languages (e.g, because it's virtual, or it only has visual assets),
    /// the first dependency with languages provides the value. If no dependency has languages, the mod's game languages are used.
    /// </remarks>
    /// <param name="playableObject">the playable object to search localizations for.</param>
    /// <returns>A collection of installed languages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public IReadOnlyCollection<ILanguageInfo> FindLanguages(IPlayableObject playableObject)
    {
        if (playableObject == null)
            throw new ArgumentNullException(nameof(playableObject));
        if (playableObject is IGame game)
            return GetInstalledGameLanguages(game);
        if (playableObject is IMod mod)
            return GetInstalledModLanguages(mod);
        return playableObject.Game.InstalledLanguages;
    }


    /// <summary>
    /// Searches for installed languages for the specified mod. If a mod has modinfo data and this data has explicitly set languages, the value from modinfo is returned as is.
    /// Also, in the case a mod does not have any languages (e.g, because it's virtual, or it only has visual assets),
    /// the first dependency with languages provides the value. If no dependency has languages, the mod's game languages are used.
    /// When no languages are found the method returns an empty collection.
    /// </summary>
    /// <remarks>
    /// The method returns an empty collection, when mod dependencies are not resolved.
    /// </remarks>
    /// <param name="mod">the mod to search localizations for.</param>
    /// <returns>A collection of installed languages.</returns>
    protected virtual IReadOnlyCollection<ILanguageInfo> GetInstalledModLanguages(IMod mod)
    {
        // We only use modinfo data, if it was explicitly set by the mod creator.
        var modinfo = mod.ModInfo;
        if (modinfo is not null && modinfo.LanguagesExplicitlySet)
            return modinfo.Languages;

        var foundLanguages = mod is not IPhysicalPlayableObject physicalPlayableObject
            ? []
            : GetLanguagesFromPhysicalLocation(physicalPlayableObject);

        if (foundLanguages.Count > 0)
            return foundLanguages;

        // Visual asset-only (sub) mods, may not have any localizations. Thus,
        // as a fallback, we want to get the languages of the first dependency which has languages.
        // NB: This strategy is not correct every time. E.g, if the next a dependency is a language pack,
        // we would not only return this. Using the last dependency is also not perfect,
        // because the next dependency may add so many changes, including localization,
        // that the original localizations from the last dependency are overwritten. 
        // In such complex cases the mod creators should always use modinfo data to express their actual intents.
        foreach (var dependency in mod.Dependencies)
        {
            var dependencyLanguages = dependency.InstalledLanguages;
            if (dependencyLanguages.Count != 0)
                return dependencyLanguages;
        }

        // In the case the whole mod chain does not have languages, use the game language
        return mod.Game.InstalledLanguages;
    }

    /// <summary>
    /// Searches for installed languages for the specified game. When no languages are found the method returns an empty collection.
    /// </summary>
    /// <param name="game">the game to search localizations for.</param>
    /// <returns>A collection of installed languages.</returns>
    protected virtual IReadOnlyCollection<ILanguageInfo> GetInstalledGameLanguages(IGame game)
    {
        return GetLanguagesFromPhysicalLocation(game);
    }

    private static IReadOnlyCollection<ILanguageInfo> GetLanguagesFromPhysicalLocation(IPhysicalPlayableObject physicalPlayable)
    {
        var text = GameLocalizationUtilities.GetTextLocalizations(physicalPlayable);
        var speech = GameLocalizationUtilities.GetSpeechLocalizationsFromMegs(physicalPlayable);
        var sfx = GameLocalizationUtilities.GetSfxMegLocalizations(physicalPlayable);
        return GameLocalizationUtilities.Merge(text, speech, sfx);
    }
}