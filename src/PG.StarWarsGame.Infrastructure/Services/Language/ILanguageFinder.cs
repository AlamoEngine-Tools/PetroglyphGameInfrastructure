using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service to identify localizations.
/// </summary>
public interface ILanguageFinder
{
    /// <summary>
    /// Searches for installed languages for the specified playable object.
    /// </summary>
    /// <param name="playableObject">the playable object to search localizations for.</param>
    /// <returns>A collection of installed languages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    IReadOnlyCollection<ILanguageInfo> FindLanguages(IPlayableObject playableObject);
}