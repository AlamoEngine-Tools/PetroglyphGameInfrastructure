using System;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Provides a very simple implementation which searches returns the first .ico file in a mod's directory.
/// For virtual mods it returns the icon of the first physical dependency which has a icon.
/// </summary>
public class IconFinder(IServiceProvider serviceProvider) : IIconFinder
{
    private const string EawIconName = "eaw.ico";
    private const string FocIconName = "foc.ico";

    private readonly IPlayableObjectFileService _fileService = serviceProvider.GetRequiredService<IPlayableObjectFileService>();
    /// <inheritdoc />
    /// <remarks>
    /// Virtual mods are currently not supported and always return <see langword="null"/>.
    /// </remarks>
    public string? FindIcon(IPlayableObject playableObject)
    {
        if (playableObject == null) 
            throw new ArgumentNullException(nameof(playableObject));

        if (playableObject is IGame game)
            return FindIconForGame(game);
        if (playableObject is IMod mod)
            return FindIconForMod(mod);
        
        throw new NotSupportedException($"The playable object of type '{playableObject.GetType().Name}' is not supported.");
    }

    /// <summary>
    /// Searches an icon for a specified mod.
    /// </summary>
    /// <param name="mod">The mod to search an icon for.</param>
    /// <returns>The path to the found icon, or <see langword="null"/> if no icon was found.</returns>
    protected virtual string? FindIconForMod(IMod mod)
    {
        if (mod is IPhysicalMod physicalMod)
            return _fileService.DataFiles(physicalMod, "*.ico", "..", false, false)
                .FirstOrDefault()?.FullName;
        if (mod.Type == ModType.Virtual)
        {
            // TODO: For now, virtual mods don't have icons
            return null;
        }
        return null;
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
        return _fileService.DataFiles(game, expectedFileName, "..", false, false)
            .FirstOrDefault()?.FullName;
    }
}