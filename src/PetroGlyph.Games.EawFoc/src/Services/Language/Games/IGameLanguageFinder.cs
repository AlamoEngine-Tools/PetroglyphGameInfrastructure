using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Service to identify which languages a game has installed.
/// </summary>
internal interface IGameLanguageFinder
{
    /// <summary>
    /// Finds all installed languages of a game.
    /// </summary>
    /// <param name="game">The target game.</param>
    /// <returns>Set of installed languages.</returns>
    ISet<ILanguageInfo> FindInstalledLanguages(IGame game);
}