using System.Collections.Generic;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service to find evidence of installed languages for <see cref="IPhysicalPlayableObject"/>s. 
/// </summary>
public interface ILanguageFinder
{
    /// <summary>
    /// Searches a given <see cref="IPhysicalPlayableObject"/> for installed text localizations.
    /// </summary>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <returns>A set of found text localization.</returns>
    ISet<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject);

    /// <summary>
    /// Searches a given <see cref="IPhysicalPlayableObject"/> for installed SFX localizations.
    /// </summary>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <returns>A set of found SFX localization.</returns>
    ISet<ILanguageInfo> GetSfxMegLocalizations(IPhysicalPlayableObject playableObject);

    /// <summary>
    /// Searches a given <see cref="IPhysicalPlayableObject"/> for installed speech localizations
    /// in Petroglyph specific languages folders..
    /// </summary>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <returns>A set of found speech localization.</returns>
    ISet<ILanguageInfo> GetSpeechLocalizationsFromFolder(IPhysicalPlayableObject playableObject);

    /// <summary>
    /// Searches a given <see cref="IPhysicalPlayableObject"/> for installed speech localizations from .meg files.
    /// </summary>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <returns>A set of found speech localization.</returns>
    ISet<ILanguageInfo> GetSpeechLocalizationsFromMegs(IPhysicalPlayableObject playableObject);

    /// <summary>
    /// Merges multiple enumerations of <see cref="ILanguageInfo"/> into a single set.
    /// Merging is done by updating the <see cref="ILanguageInfo.Support"/> flag.
    /// The resulting set contains every <see cref="ILanguageInfo.Code"/> exactly once with merged support flag.
    /// </summary>
    /// <example>So if the first enumerable contains a language {German:Text}
    /// and another enumerable contains a language {German:Speech | SFX}
    /// gets merged to {German:FullLocalized}</example>
    /// <param name="setsToMerge">The enumerations to merge</param>
    /// <returns>The merged set.</returns>
    ISet<ILanguageInfo> Merge(params IEnumerable<ILanguageInfo>[] setsToMerge);
}