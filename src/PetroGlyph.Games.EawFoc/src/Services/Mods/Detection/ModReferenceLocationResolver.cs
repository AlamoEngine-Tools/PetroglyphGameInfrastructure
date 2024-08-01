using System;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <inheritdoc cref="IModReferenceLocationResolver"/>
internal class ModReferenceLocationResolver : IModReferenceLocationResolver
{
    private readonly ISteamGameHelpers _steamHelper;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModReferenceLocationResolver(IServiceProvider serviceProvider)
    {
        _steamHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    }

    /// <inheritdoc/>
    public IDirectoryInfo ResolveLocation(IModReference mod, IGame game)
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
            _ => mod.Type == ModType.Workshops ? ResolveWorkshopsMod(mod, game) : ResolveNormalMod(mod, game)
        };
    }

    private IDirectoryInfo ResolveWorkshopsMod(IModReference mod, IGame game)
    {
        var workshopPath = _steamHelper.GetWorkshopsLocation(game);
        if (!workshopPath.Exists)
            throw new GameException("Could not find workshops location");

        if (!_steamHelper.ToSteamWorkshopsId(mod.Identifier, out var steamId))
            throw new GameException("Mod identifier cannot be interpreted as an Steam-ID");

        var modLocation = workshopPath.EnumerateDirectories(steamId.ToString()).FirstOrDefault();
        if (modLocation is null || !modLocation.Exists)
            throw new ModNotFoundException(mod, game);

        return modLocation;
    }

    private static IDirectoryInfo ResolveNormalMod(IModReference mod, IGame game)
    {
        var fs = game.Directory.FileSystem;
        var modIdentifier = mod.Identifier;

        if (!fs.Path.IsPathFullyQualified(game.Directory.FullName))
            throw new GameException("Game path must be absolute");

        // Note about mod paths:
        // The path can be absolute or relative. 
        //  a) If absolute the mod may be located anywhere, even on different volumes.
        //  b) If relative the path must be relative to the game's root directory.
        // For starting mods the path also must not contain any spaces, but that's not assured here,
        // because may have custom features to still support these mods. 

        if (fs.Path.IsPathFullyQualified(modIdentifier))
            return fs.DirectoryInfo.New(modIdentifier);

        var modLocationPath = fs.Path.Combine(game.Directory.FullName, modIdentifier);
        var modLocation = fs.DirectoryInfo.New(modLocationPath);
        if (modLocation is null || !modLocation.Exists)
            throw new ModNotFoundException(mod, game);
        return modLocation;
    }
}