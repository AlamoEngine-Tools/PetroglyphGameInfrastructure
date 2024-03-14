using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <inheritdoc cref="ISteamGameHelpers"/>
public class SteamGameHelpers : ISteamGameHelpers
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

        if (!_fileSystem.Path.IsPathFullyQualified(game.Directory.FullName))
            throw new InvalidOperationException("Game path must be absolute");

        var gameDir = game.Directory;

        var commonParent = gameDir.Parent?.Parent?.Parent;
        if (commonParent is null)
            throw new SteamException("Unable to compute workshops location");

        var fs = game.Directory.FileSystem;
        var workshopDirPath = fs.Path.Combine(commonParent.FullName, "workshop/content/32470");
        return fs.DirectoryInfo.New(workshopDirPath);
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