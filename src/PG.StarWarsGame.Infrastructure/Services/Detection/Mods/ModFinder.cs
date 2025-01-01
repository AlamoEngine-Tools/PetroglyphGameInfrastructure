using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using EawModinfo.File;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

internal class ModFinder(IServiceProvider serviceProvider) : IModFinder
{
    private readonly ISteamGameHelpers _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    private readonly IModGameTypeResolver _gameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModFinder));

    public IEnumerable<DetectedModReference> FindMods(IGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));
        if (!game.Exists())
            throw new GameException("The game does not exist");
        _logger?.LogDebug($"Searching mods for game '{game}'");
        return GetNormalMods(game).Union(GetWorkshopsMods(game));
    }

    public IEnumerable<DetectedModReference> FindMods(IGame game, IDirectoryInfo directory)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (directory == null) 
            throw new ArgumentNullException(nameof(directory));
        if (!game.Exists())
            throw new GameException($"The game '{game}' does not exist");

        var modLocationKind = GetModLocationKind(game, directory);
        _logger?.LogDebug($"Searching mods with at location '{directory.FullName}' of location kind '{modLocationKind}' for game '{game}'");
        return GetModsFromDirectory(directory, modLocationKind, game.Type);
    }

    private IEnumerable<DetectedModReference> GetNormalMods(IGame game)
    {
        _logger?.LogTrace($"Searching normal mods for game '{game}'");
        return GetAllModsFromContainerPath(game.ModsLocation, ModReferenceBuilder.ModLocationKind.GameModsDirectory, game.Type);
    }

    private IEnumerable<DetectedModReference> GetWorkshopsMods(IGame game)
    {
        if (game.Platform != GamePlatform.SteamGold)
            return [];

        _logger?.LogTrace($"Searching Steam Workshop mods for game '{game}'");
        return GetAllModsFromContainerPath(_steamHelper.GetWorkshopsLocation(game),
            ModReferenceBuilder.ModLocationKind.SteamWorkshops, game.Type);
    }

    private IEnumerable<DetectedModReference> GetAllModsFromContainerPath(IDirectoryInfo lookupDirectory, ModReferenceBuilder.ModLocationKind locationKind, GameType requestedGameType)
    {
        if (!lookupDirectory.Exists)
            return [];

        return lookupDirectory.EnumerateDirectories()
            .SelectMany(x => GetModsFromDirectory(x, locationKind, requestedGameType));
    }


    private IEnumerable<DetectedModReference> GetModsFromDirectory(
        IDirectoryInfo modDirectory, 
        ModReferenceBuilder.ModLocationKind locationKind,
        GameType requestedGameType)
    {
        if (!modDirectory.Exists)
            yield break;

        if (locationKind == ModReferenceBuilder.ModLocationKind.SteamWorkshops && !_steamHelper.ToSteamWorkshopsId(modDirectory.Name, out _))
            yield break;

        _logger?.LogTrace($"Searching for mods at location '{modDirectory.FullName}'");

        ModinfoFinderCollection modinfoFiles;
        modinfoFiles = ModinfoFileFinder.FindModinfoFiles(modDirectory);
        
        foreach (var modRef in ModReferenceBuilder.CreateIdentifiers(modinfoFiles, locationKind))
        {
            if (_gameTypeResolver.IsDefinitelyNotCompatibleToGame(modRef, requestedGameType))
            {
                _logger?.LogTrace($"Skipping mod reference '{modRef.ModReference}' because it is not compatible to '{requestedGameType}'");
                continue;
            }
            yield return modRef;
        }
    }

    private ModReferenceBuilder.ModLocationKind GetModLocationKind(IGame game, IDirectoryInfo directory)
    {
        var gameModsPath = game.ModsLocation.FullName;
        var modPath = directory.FullName;

        if (_fileSystem.Path.IsChildOf(gameModsPath, modPath))
            return ModReferenceBuilder.ModLocationKind.GameModsDirectory;

        if (game.Platform is not GamePlatform.SteamGold)
            return ModReferenceBuilder.ModLocationKind.External;

        if (!_steamHelper.ToSteamWorkshopsId(directory.Name, out _))
            return ModReferenceBuilder.ModLocationKind.External;

        if (!_steamHelper.TryGetWorkshopsLocation(game, out var steamWsDir))
            return ModReferenceBuilder.ModLocationKind.External;

        if (_fileSystem.Path.IsChildOf(steamWsDir.FullName, modPath))
            return ModReferenceBuilder.ModLocationKind.SteamWorkshops;

        return ModReferenceBuilder.ModLocationKind.External;
    }
}