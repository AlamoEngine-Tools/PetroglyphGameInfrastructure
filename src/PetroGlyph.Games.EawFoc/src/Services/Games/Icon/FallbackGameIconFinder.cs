using System;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Provides a fallback implementation which searches a game's icon file in its root directory.
/// </summary>
public class FallbackGameIconFinder : IGameIconFinder
{
    private const string EawIconName = "eaw.ico";
    private const string FocIconName = "foc.ico";

    /// <summary>
    /// Searches for hardcoded icon names.
    /// "eaw.ico" for Empire at War and
    /// "foc.ico" for Forces of Corruption
    /// </summary>
    public string? FindIcon(IGame game)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        var expectedFileName = game.Type switch
        {
            GameType.Eaw => EawIconName,
            GameType.Foc => FocIconName,
            _ => throw new ArgumentOutOfRangeException()
        };
        return game.FileService.DataFiles(expectedFileName, "..", false, false).FirstOrDefault()?.FullName;
    }
}