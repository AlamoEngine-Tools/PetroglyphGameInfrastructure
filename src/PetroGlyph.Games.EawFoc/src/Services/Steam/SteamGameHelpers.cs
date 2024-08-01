using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <inheritdoc cref="ISteamGameHelpers"/>
internal class SteamGameHelpers : ISteamGameHelpers
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SteamGameHelpers(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    /// <inheritdoc/>
    public IDirectoryInfo GetWorkshopsLocation(IGame game)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (game.Platform != GamePlatform.SteamGold)
            throw new GameException("Unable to get workshops location for non-Steam game.");

        var gameDir = game.Directory;

        var commonParent = gameDir.Parent?.Parent?.Parent;
        if (commonParent is null)
            throw new GameException("Unable to get workshops location.");

        var workshopDirPath = _fileSystem.Path.Combine(commonParent.FullName, "workshop/content/32470");
        return _fileSystem.DirectoryInfo.New(workshopDirPath);
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
        catch (PetroglyphException)
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