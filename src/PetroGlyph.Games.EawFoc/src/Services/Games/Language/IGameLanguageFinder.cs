using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Language;

/// <summary>
/// Service to identify which languages a game has installed.
/// </summary>
public interface IGameLanguageFinder
{
    /// <summary>
    /// Finds all installed languages of a game.
    /// </summary>
    /// <param name="game">The target game.</param>
    /// <returns>Set of installed languages.</returns>
    ISet<ILanguageInfo> FindInstalledLanguages(IGame game);
}