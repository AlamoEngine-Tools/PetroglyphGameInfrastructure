using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.File;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

internal class ModFinder : IModFinder
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly IModGameTypeResolver _gameTypeResolver;

    public ModFinder(IServiceProvider serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _gameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
    }

    public ICollection<DetectedModReference> FindMods(IGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        if (!game.Exists())
            throw new GameException("The game does not exist");

        return GetNormalMods(game).Union(GetWorkshopsMods(game)).ToList();
    }

    private IEnumerable<DetectedModReference> GetNormalMods(IGame game)
    {
        return GetAllModsFromPath(game.ModsLocation, ModReferenceBuilder.ModLocationKind.GameModsDirectory, game.Type);
    }

    private IEnumerable<DetectedModReference> GetWorkshopsMods(IGame game)
    {
        return game.Platform != GamePlatform.SteamGold
            ? []
            : GetAllModsFromPath(_steamHelper.GetWorkshopsLocation(game), ModReferenceBuilder.ModLocationKind.SteamWorkshops, game.Type);
    }

    private IEnumerable<DetectedModReference> GetAllModsFromPath(IDirectoryInfo lookupDirectory, ModReferenceBuilder.ModLocationKind locationKind, GameType requestedGameType)
    {
        if (!lookupDirectory.Exists)
            yield break;

        foreach (var modDirectory in lookupDirectory.EnumerateDirectories())
        {
            ModinfoFinderCollection modinfoFiles;
            modinfoFiles = ModinfoFileFinder.FindModinfoFiles(modDirectory);

            foreach (var modRef in ModReferenceBuilder.CreateIdentifiers(modinfoFiles, locationKind))
            {
                if (IsDefinitelyNotGameType(requestedGameType, modDirectory, modRef.ModReference.Type, modRef.Modinfo))
                    continue;
                yield return modRef;
            }
        }
    }

    private bool IsDefinitelyNotGameType(GameType expected, IDirectoryInfo modDirectory, ModType modType, IModinfo? modinfo)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return _gameTypeResolver.TryGetGameType(modDirectory, modType, modinfo, out var variantGameType) &&
               !variantGameType.Contains(expected);
    }
}