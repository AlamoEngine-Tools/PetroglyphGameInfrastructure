using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language;

/// <summary>
/// Service to get installed mods. Implementations are aware of the modinfo specification
/// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#the-languages-property"/>
/// </summary>
public interface IModLanguageFinder
{
    /// <summary>
    /// Finds all installed languages for this <see cref="IMod"/> instance.
    /// </summary>
    /// <param name="mod">The target mod</param>
    /// <returns>A set of installed language information.</returns>
    ISet<ILanguageInfo> FindInstalledLanguages(IMod mod);
}