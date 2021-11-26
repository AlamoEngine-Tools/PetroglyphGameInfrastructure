using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using Sklavenwalker.CommonUtilities.FileSystem;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Steam;

/// <inheritdoc cref="ISteamGameHelpers"/>
public class SteamGameHelpers : ISteamGameHelpers
{
    private readonly IPathHelperService _pathHelper;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SteamGameHelpers(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        var fs = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
        _pathHelper = serviceProvider.GetService<IPathHelperService>() ?? new PathHelperService(fs);
    }

    /// <inheritdoc/>
    public IDirectoryInfo GetWorkshopsLocation(IGame game)
    {
        Requires.NotNull(game, nameof(game));
        if (game.Platform != GamePlatform.SteamGold)
            throw new GameException("Unable to get workshops location for non-Steam game.");

        if (!_pathHelper.IsAbsolute(game.Directory.FullName))
            throw new InvalidOperationException("Game path must be absolute");

        var gameDir = game.Directory;

        var commonParent = gameDir.Parent?.Parent?.Parent;
        if (commonParent is null)
            throw new SteamException("Unable to compute workshops location");

        var fs = game.Directory.FileSystem;
        var workshopDirPath = fs.Path.Combine(commonParent.FullName, "workshop/content/32470");
        return fs.DirectoryInfo.FromDirectoryName(workshopDirPath);
    }

    /// <inheritdoc/>
    public bool TryGetWorkshopsLocation(IGame game, out IDirectoryInfo? workshopsLocation)
    {
        workshopsLocation = null;
        try
        {
            workshopsLocation = GetWorkshopsLocation(game);
            return true;
        }
        catch (Exception e) when (e is PetroglyphException or SteamException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool ToSteamWorkshopsId(string input, out ulong steamId)
    {
        return ulong.TryParse(input, out steamId);
    }
}