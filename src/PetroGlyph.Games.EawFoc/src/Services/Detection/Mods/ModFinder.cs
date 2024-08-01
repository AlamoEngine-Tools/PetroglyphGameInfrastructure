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
    }

    /// <summary>
    /// Searches mods for the given <paramref name="game"/>.
    /// </summary>
    /// <param name="game">The game which hosts the mods.</param>
    /// <returns>A set of <see cref="IModReference"/>s.
    /// The <see cref="IModReference.Identifier"/> either holds the absolute path or Steam Workshops ID.</returns>
    /// <exception cref="GameException">If the <paramref name="game"/> is not installed.</exception>
    public ISet<IModReference> FindMods(IGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        if (!game.Exists())
            throw new GameException("The game does not exist");

        var mods = new HashSet<IModReference>();
        foreach (var modReference in GetNormalMods(game).Union(GetWorkshopsMods(game)))
            mods.Add(modReference);
        return mods;
    }

    private IEnumerable<ModReference> GetNormalMods(IGame game)
    {
        return GetAllModsFromPath(game.ModsLocation, false);
    }

    private IEnumerable<ModReference> GetWorkshopsMods(IGame game)
    {
        return game.Platform != GamePlatform.SteamGold
            ? []
            : GetAllModsFromPath(_steamHelper.GetWorkshopsLocation(game), true);
    }

    private IEnumerable<ModReference> GetAllModsFromPath(IDirectoryInfo lookupDirectory, bool isWorkshopsPath)
    {
        if (!lookupDirectory.Exists)
            yield break;

        var type = isWorkshopsPath ? ModType.Workshops : ModType.Default;

        foreach (var modDirectory in lookupDirectory.EnumerateDirectories())
        {
            var id = _idBuilder.Build(modDirectory, isWorkshopsPath);
            yield return new ModReference(id, type);
        }
    }
}