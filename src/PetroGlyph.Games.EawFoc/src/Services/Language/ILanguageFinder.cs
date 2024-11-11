using System;
using System.Collections.Generic;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Language;


/// <summary>
/// Service to identify localizations.
/// </summary>
public interface ILanguageFinder
{
    /// <summary>
    /// Searches for installed languages for the specified playable object.
    /// </summary>
    /// <param name="playableObject">the playable object to search localization for.</param>
    /// <param name="inheritFromDependencies">
    /// When set to <see langword="true"/>, the language lookup also uses the languages from the object's dependencies. 
    /// Default is <see langword="false"/>. The option is only application to mods.
    /// </param>
    /// <returns>A collection of installed languages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    IReadOnlyCollection<ILanguageInfo> FindLanguages(IPlayableObject playableObject, bool inheritFromDependencies = false);
}