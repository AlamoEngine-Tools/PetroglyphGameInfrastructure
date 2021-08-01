using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Steam
{
    /// <summary>
    /// Common helpers for Steam-based Games
    /// </summary>
    public static class SteamGameHelpers
    {
        /// <summary>
        /// Get's the game's workshop directory.
        /// </summary>
        /// <param name="game">The target game</param>
        /// <exception cref="GameException">If the game is not a Steam game</exception>
        /// <exception cref="SteamException">If it was impossible to compute the workshop location.</exception>
        /// <exception cref="InvalidOperationException">If the game's directory info is not absolute.</exception>
        public static IDirectoryInfo GetWorkshopsLocation(IGame game)
        {
            Requires.NotNull(game, nameof(game));
            if (game.Platform != GamePlatform.SteamGold)
                throw new GameException("Unable to get workshops location for non-Steam game.");

            if (!PathUtilities.IsAbsolute(game.Directory.FullName))
                throw new InvalidOperationException("Game path must be absolute");

            var gameDir = game.Directory;

            var commonParent = gameDir.Parent?.Parent?.Parent;
            if (commonParent is null)
                throw new SteamException("Unable to compute workshops location");

            var fs = game.Directory.FileSystem;
            var workshopDirPath = fs.Path.Combine(commonParent.FullName, "workshop/content/32470");
            return fs.DirectoryInfo.FromDirectoryName(workshopDirPath);
        }

        /// <summary>
        /// Tries to get the game's workshop location.
        /// </summary>
        /// <param name="game">The target game.</param>
        /// <param name="workshopsLocation">The found location; <see langword="null"/> of no workshop location could be computed.</param>
        /// <returns><see langword="true"/>if the workshop location was found; <see langword="false"/> otherwise.</returns>
        public static bool TryGetWorkshopsLocation(IGame game, out IDirectoryInfo? workshopsLocation)
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

        /// <summary>
        /// Tries to convert a string to a <see cref="ulong"/> value which acts as a SteamWorkshop ID. 
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="steamId">The resulting id.</param>
        /// <returns><see langword="true"/>if the <paramref name="input"/> could be converted; <see langword="false"/> otherwise.</returns>
        public static bool ToSteamWorkshopsId(string input, out ulong steamId)
        {
            return ulong.TryParse(input, out steamId);
        }
    }
}