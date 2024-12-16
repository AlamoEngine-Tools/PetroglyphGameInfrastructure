using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using EawModinfo.File;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

internal class ModFinder(IServiceProvider serviceProvider) : IModFinder
{
    private readonly ISteamGameHelpers _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    private readonly IModGameTypeResolver _gameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public IEnumerable<DetectedModReference> FindMods(IGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));
        if (!game.Exists())
            throw new GameException("The game does not exist");
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
        return GetModsFromDirectory(directory, modLocationKind, game.Type);
    }

    private IEnumerable<DetectedModReference> GetNormalMods(IGame game)
    {
        return GetAllModsFromContainerPath(game.ModsLocation, ModReferenceBuilder.ModLocationKind.GameModsDirectory, game.Type);
    }

    private IEnumerable<DetectedModReference> GetWorkshopsMods(IGame game)
    {
        return game.Platform != GamePlatform.SteamGold
            ? []
            : GetAllModsFromContainerPath(_steamHelper.GetWorkshopsLocation(game), ModReferenceBuilder.ModLocationKind.SteamWorkshops, game.Type);
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

        ModinfoFinderCollection modinfoFiles;
        modinfoFiles = ModinfoFileFinder.FindModinfoFiles(modDirectory);
        
        foreach (var modRef in ModReferenceBuilder.CreateIdentifiers(modinfoFiles, locationKind))
        {
            if (_gameTypeResolver.IsDefinitelyNotCompatibleToGame(modRef, requestedGameType))
                continue;
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