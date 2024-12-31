using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

/// <inheritdoc cref="ISteamGameHelpers"/>
internal class SteamGameHelpers(IServiceProvider serviceProvider) : ISteamGameHelpers
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

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

        var workshopDirPath = _fileSystem.Path.Combine(commonParent.FullName, "workshop", "content", "32470");
        return _fileSystem.DirectoryInfo.New(workshopDirPath);
    }

    /// <inheritdoc/>
    public bool TryGetWorkshopsLocation(IGame game, [NotNullWhen(true)] out IDirectoryInfo? workshopsLocation)
    {
        workshopsLocation = null;
        try
        {
            workshopsLocation = GetWorkshopsLocation(game);
            return true;
        }
        catch(GameException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool ToSteamWorkshopsId(string input, out ulong steamId)
    {
        return IstValidSteamWorkshopsDir(input, out steamId);
    }

    public static bool IstValidSteamWorkshopsDir(string input, out ulong steamId)
    {
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(input);
        return ulong.TryParse(input, NumberStyles.None, null, out steamId);
    }

    public static bool IstValidSteamWorkshopsDir(string input)
    {
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(input);
        return IstValidSteamWorkshopsDir(input, out _);
    }
}