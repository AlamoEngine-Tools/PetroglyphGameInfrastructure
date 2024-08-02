using System;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <inheritdoc cref="IModReferenceLocationResolver"/>
internal class ModReferenceLocationResolver : IModReferenceLocationResolver
{
    private readonly IFileSystem _fileSystem;
    private readonly ISteamGameHelpers _steamHelper;
    private readonly IModGameTypeResolver _modGameTypeResolver;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModReferenceLocationResolver(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        _modGameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
    }

    /// <inheritdoc/>
    public IDirectoryInfo ResolveLocation(IModReference mod, IGame game, bool checkModGameType)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        return mod.Type switch
        {
            ModType.Virtual => throw new NotSupportedException(
                "Resolving physical location for a virtual mod is not allowed."),
            ModType.Workshops when game.Platform != GamePlatform.SteamGold => throw new ModException(mod,
                "Trying to find a workshop mods for a non-Steam game instance"),
            _ => mod.Type == ModType.Workshops ? ResolveWorkshopsMod(mod, game, checkModGameType) : ResolveNormalMod(mod, game, checkModGameType)
        };
    }

    private IDirectoryInfo ResolveWorkshopsMod(IModReference mod, IGame game, bool checkGameType)
    {
        if (mod is IPhysicalMod physicalMod)
        {
            if (checkGameType && game.Type != physicalMod.Game.Type)
                throw new ModException(mod, $"The mod '{mod}' does not belong to the game '{game}'.");
            return physicalMod.Directory;
        }

        var workshopPath = _steamHelper.GetWorkshopsLocation(game);
        if (!workshopPath.Exists)
            throw new GameException("Could not find workshops location");

        if (!_steamHelper.ToSteamWorkshopsId(mod.Identifier, out var steamId))
            throw new ModException(mod, "Mod identifier cannot be interpreted as an Steam-ID");

        var modLocation = workshopPath.EnumerateDirectories(steamId.ToString()).FirstOrDefault();
        if (modLocation is null || !modLocation.Exists)
            throw new ModNotFoundException(mod, game);

        if (checkGameType) 
            CheckModGameType(mod, modLocation, ModType.Workshops, game);


        return modLocation;
    }

    private IDirectoryInfo ResolveNormalMod(IModReference mod, IGame game, bool checkGameType)
    {
        if (mod is IPhysicalMod physicalMod)
        {
            if (checkGameType && game.Type != physicalMod.Game.Type)
                throw new ModException(mod, $"The mod '{mod}' does not belong to the game '{game}'.");
            return physicalMod.Directory;
        }

        var modIdentifier = mod.Identifier;

        // Note about mod paths:
        // The path can be absolute or relative. 
        //  a) If absolute the mod may be located anywhere, even on different volumes.
        //  b) If relative the path must be relative to the game's root directory.
        // For starting mods the path also must not contain any spaces, but that's not assured here.


        IDirectoryInfo modLocation;
        if (_fileSystem.Path.IsPathFullyQualified(modIdentifier))
            modLocation = _fileSystem.DirectoryInfo.New(modIdentifier);
        else
            modLocation = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(game.Directory.FullName, modIdentifier));
        
        if (modLocation is null || !modLocation.Exists)
            throw new ModNotFoundException(mod, game);

        if (checkGameType) 
            CheckModGameType(mod, modLocation, ModType.Default, game);

        return modLocation;
    }

    private void CheckModGameType(IModReference mod, IDirectoryInfo modLocation, ModType modType, IGame game)
    {
        if (_modGameTypeResolver.TryGetGameType(modLocation, modType, true, out var gameType) && gameType != game.Type)
            throw new ModException(mod, $"The mod '{mod}' does not belong to the game '{game}'.");
    }
}