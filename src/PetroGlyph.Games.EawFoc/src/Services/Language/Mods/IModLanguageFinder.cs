using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service to get installed mods. Implementations are aware of the modinfo specification
/// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#the-languages-property"/>
/// </summary>
internal interface IModLanguageFinder
{
    /// <summary>
    /// Finds all installed languages for this <see cref="IMod"/> instance.
    /// </summary>
    /// <param name="mod">The target mod</param>
    /// <returns>A set of installed language information.</returns>
    IReadOnlyCollection<ILanguageInfo> FindInstalledLanguages(IMod mod);
}