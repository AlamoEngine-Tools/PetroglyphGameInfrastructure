using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service that searches installed localizations for playable objects.
/// </summary>
/// <param name="serviceProvider"></param>
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
    /// For mods: If a mod has modinfo data and this data has specified languages other than the default collection (English - FullLocalized),
    /// the value from modinfo is returned as is.
    /// </remarks>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <param name="inheritFromDependencies">
    /// When set to <see langword="true"/>, the language lookup also uses the languages from the object's dependencies. 
    /// Default is <see langword="false"/>. The option is only application to mods.
    /// </param>
    /// <returns>A collection of installed languages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public IReadOnlyCollection<ILanguageInfo> FindLanguages(IPlayableObject playableObject, bool inheritFromDependencies = false)
    {
        if (playableObject == null)
            throw new ArgumentNullException(nameof(playableObject));
        if (playableObject is IGame game)
            return GetInstalledGameLanguages(game);
        if (playableObject is IMod mod)
            return GetInstalledModLanguages(mod, inheritFromDependencies);
        throw new NotSupportedException($"The playable object of type '{playableObject.GetType().Name}' is not supported.");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="inheritFromDependencies"></param>
    /// <returns></returns>
    protected virtual IReadOnlyCollection<ILanguageInfo> GetInstalledModLanguages(IMod mod, bool inheritFromDependencies)
    {
        // We don't "trust" the modinfo here as the default language (EN) also gets
        // applied when nothing was specified by the mod developer.
        // Only if we have more than the default language, we trust the modinfo.
        var modinfo = mod.ModInfo;
        if (modinfo is not null && !IsEmptyOrDefault(modinfo.Languages))
            return modinfo.Languages;

        var foundLanguages = mod is not IPhysicalPlayableObject physicalPlayableObject ? [] : GetLanguagesFromPhysicalLocation(physicalPlayableObject);
        
        if (!IsEmptyOrDefault(foundLanguages))
            return foundLanguages;

        if (!inheritFromDependencies || mod.DependencyResolveStatus != DependencyResolveStatus.Resolved)
            return foundLanguages;

        foreach (var dependency in mod.Dependencies)
        {
            var dependencyLanguages = dependency.Mod.InstalledLanguages;
            if (!IsEmptyOrDefault(dependencyLanguages))
                return dependencyLanguages;
        }
        return foundLanguages;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    protected virtual IReadOnlyCollection<ILanguageInfo> GetInstalledGameLanguages(IGame game)
    {
        return GetLanguagesFromPhysicalLocation(game);
    }

    private IReadOnlyCollection<ILanguageInfo> GetLanguagesFromPhysicalLocation(IPhysicalPlayableObject physicalPlayable)
    {
        var text = GameLocalizationUtilities.GetTextLocalizations(physicalPlayable);
        var speech = GameLocalizationUtilities.GetSpeechLocalizationsFromMegs(physicalPlayable);
        var sfx = GameLocalizationUtilities.GetSfxMegLocalizations(physicalPlayable);
        return GameLocalizationUtilities.Merge(text, speech, sfx);
    }

    private static bool IsEmptyOrDefault(IReadOnlyCollection<ILanguageInfo> languages)
    {
        switch (languages.Count)
        {
            case 0:
            case 1 when languages.First().Equals(LanguageInfo.Default):
                return true;
            default:
                return false;
        }
    }
}