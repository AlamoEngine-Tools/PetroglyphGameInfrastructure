using System;
using System.Linq;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Searches icon locations for playable objects.
/// </summary>
public class IconFinder : IIconFinder
{
    private const string EawIconName = "eaw.ico";
    private const string FocIconName = "foc.ico";

    /// <inheritdoc />
    /// <remarks>
    /// For Mods: If a mod has modinfo data and <see cref="IModinfo.Icon"/> is not <see langword="null"/> and not empty, this value is taken.
    /// Otherwise, icons are searched from the file system. As a fallback the icon from the game is used, 
    /// or <see langword="null"/> is returned if no icon is found.
    /// </remarks>
    public string? FindIcon(IPlayableObject playableObject)
    {
        if (playableObject == null) 
            throw new ArgumentNullException(nameof(playableObject));
        
        if (playableObject is IGame game)
            return FindIconForGame(game);
        if (playableObject is IMod mod)
            return FindIconForMod(mod);

        return playableObject.Game.IconFile;
    }

    /// <summary>
    /// Searches an icon for a specified mod.
    /// </summary>
    /// <param name="mod">The mod to search an icon for.</param>
    /// <returns>The path to the found icon, or <see langword="null"/> if no icon was found.</returns>
    protected virtual string? FindIconForMod(IMod mod)
    {
        var iconFile = mod.ModInfo?.Icon;
        if (!string.IsNullOrEmpty(iconFile))
            return iconFile;

        if (mod is IPhysicalMod physicalMod)
            iconFile = physicalMod.DataFiles("*.ico", "..", false)
                .FirstOrDefault()?.FullName;

        return iconFile ?? mod.Game.IconFile;
    }

    /// <summary>
    /// Searches an icon for a specified game.
    /// </summary>
    /// <param name="game">The game to search an icon for.</param>
    /// <returns>The path to the found icon, or <see langword="null"/> if no icon was found.</returns>
    protected virtual string? FindIconForGame(IGame game)
    {
        var expectedFileName = game.Type switch
        {
            GameType.Eaw => EawIconName,
            GameType.Foc => FocIconName,
            _ => throw new ArgumentOutOfRangeException()
        };
        return game.EnumerateFiles(expectedFileName, null, false)
            .FirstOrDefault()?.FullName;
    }
}