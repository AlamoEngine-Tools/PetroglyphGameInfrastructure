using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Searches for <see cref="IModReference"/>s for a give <see cref="IGame"/>.
/// </summary>
internal class ModFinder : IModReferenceFinder
{
    private readonly ISteamGameHelpers _steamHelper;
    private readonly IModIdentifierBuilder _idBuilder;
    private readonly IModGameTypeResolver _gameTypeResolver;
    private readonly IModinfoFileFinder _modinfoFileFinder;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public ModFinder(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        _idBuilder = serviceProvider.GetRequiredService<IModIdentifierBuilder>();
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _gameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
        _modinfoFileFinder = serviceProvider.GetRequiredService<IModinfoFileFinder>();
    }

    /// <summary>
    /// Searches mods for the given <paramref name="game"/>.
    /// </summary>
    /// <param name="game">The game which hosts the mods.</param>
    /// <returns>A set of <see cref="IModReference"/>s.
    /// The <see cref="IModReference.Identifier"/> either holds the absolute path or Steam Workshops ID.</returns>
    /// <exception cref="GameException">If the <paramref name="game"/> is not installed.</exception>
    public ISet<DetectedModReference> FindMods(IGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        if (!game.Exists())
            throw new GameException("The game does not exist");

        var mods = new HashSet<DetectedModReference>();
        foreach (var modReference in GetNormalMods(game).Union(GetWorkshopsMods(game)))
            mods.Add(modReference);
        return mods;
    }

    private IEnumerable<DetectedModReference> GetNormalMods(IGame game)
    {
        return GetAllModsFromPath(game.ModsLocation, false, game.Type);
    }

    private IEnumerable<DetectedModReference> GetWorkshopsMods(IGame game)
    {
        return game.Platform != GamePlatform.SteamGold
            ? []
            : GetAllModsFromPath(_steamHelper.GetWorkshopsLocation(game), true, game.Type);
    }

    private IEnumerable<DetectedModReference> GetAllModsFromPath(IDirectoryInfo lookupDirectory, bool isWorkshopsPath, GameType requestedGameType)
    {
        if (!lookupDirectory.Exists)
            yield break;

        var type = isWorkshopsPath ? ModType.Workshops : ModType.Default;

        foreach (var modDirectory in lookupDirectory.EnumerateDirectories())
        {
            var modinfoFiles = _modinfoFileFinder.Find(FindOptions.FindAny);
            
            var mainModinfo = modinfoFiles.MainModinfo?.TryGetModinfo();

            if (IsDefinitelyNotGameType(requestedGameType, modDirectory, type, mainModinfo))
                continue;

            var id = _idBuilder.Build(modDirectory, isWorkshopsPath);
            yield return new DetectedModReference(new ModReference(id, type), mainModinfo);

            foreach (var variantModinfoFile in modinfoFiles.Variants)
            {
                var variantModinfo = variantModinfoFile.TryGetModinfo();
                if (variantModinfo is null)
                    continue;


                if (IsDefinitelyNotGameType(requestedGameType, modDirectory, type, variantModinfo))
                    continue;

                id = _idBuilder.Build(modDirectory, isWorkshopsPath);
                yield return new DetectedModReference(new ModReference(id, type), variantModinfo);
            }
        }
    }

    private bool IsDefinitelyNotGameType(GameType expected, IDirectoryInfo modDirectory, ModType modType, IModinfo? modinfo)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        return _gameTypeResolver.TryGetGameType(modDirectory, modType, modinfo, out var variantGameType) &&
               expected != variantGameType;
    }
}