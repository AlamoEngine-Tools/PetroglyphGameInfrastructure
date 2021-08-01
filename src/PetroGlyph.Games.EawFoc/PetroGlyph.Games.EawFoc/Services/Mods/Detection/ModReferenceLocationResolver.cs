﻿using System;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Steam;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <inheritdoc cref="IModReferenceLocationResolver"/>
    public sealed class ModReferenceLocationResolver : IModReferenceLocationResolver
    {
        /// <inheritdoc/>
        public IDirectoryInfo ResolveLocation(IModReference mod, IGame game)
        {
            Requires.NotNull(mod, nameof(mod));
            Requires.NotNull(game, nameof(game));

            return mod.Type switch
            {
                ModType.Virtual => throw new NotSupportedException(
                    "Resolving physical location for a virtual mod is not allowed."),
                ModType.Workshops when game.Platform != GamePlatform.SteamGold => throw new ModException(
                    "Trying to find a workshop mods for a non-Steam game instance"),
                _ => mod.Type == ModType.Workshops ? ResolveWorkshopsMod(mod, game) : ResolveNormalMod(mod, game)
            };
        }

        private static IDirectoryInfo ResolveWorkshopsMod(IModReference mod, IGame game)
        {
            var workshopPath = SteamGameHelpers.GetWorkshopsLocation(game);
            if (!workshopPath.Exists)
                throw new SteamException("Could not find workshops location");

            if (!SteamGameHelpers.ToSteamWorkshopsId(mod.Identifier, out var steamId))
                throw new SteamException("Mod identifier cannot be interpreted as an Steam-ID");

            var modLocation = workshopPath.EnumerateDirectories(steamId.ToString()).FirstOrDefault();
            if (modLocation is null || !modLocation.Exists)
                throw new ModNotFoundException(mod, game);

            return modLocation;
        }

        private static IDirectoryInfo ResolveNormalMod(IModReference mod, IGame game)
        {
            if (!game.Directory.FileSystem.IsValidDirectoryPath(mod.Identifier))
                throw new ModException("Mod identifier cannot be interpreted as an absolute or relative path");

            if (!PathUtilities.IsAbsolute(game.Directory.FullName))
                throw new GameException("Game path must be absolute");


            var fs = game.Directory.FileSystem;
            var modIdentifier = mod.Identifier;
            if (PathUtilities.IsAbsolute(modIdentifier))
            {
                if (!PathUtilities.IsChildOf(game.Directory.FullName, modIdentifier))
                    throw new PetroglyphException("Mod and game must share the same path.");
                return fs.DirectoryInfo.FromDirectoryName(modIdentifier);
            }

            var modLocationPath = fs.Path.Combine(game.Directory.FullName, modIdentifier);
            var modLocation = fs.DirectoryInfo.FromDirectoryName(modLocationPath);
            if (modLocation is null || !modLocation.Exists)
                throw new ModNotFoundException(mod, game);
            return modLocation;
        }
    }
}