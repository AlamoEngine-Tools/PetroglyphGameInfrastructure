using System;
using System.Linq;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Icon;

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
        Requires.NotNull(game, nameof(game));
        var expectedFileName = game.Type switch
        {
            GameType.EaW => EawIconName,
            GameType.Foc => FocIconName,
            _ => throw new ArgumentOutOfRangeException()
        };
        return game.FileService.DataFiles(expectedFileName, "..", false, false).FirstOrDefault()?.FullName;
    }
}