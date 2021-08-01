using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Steam;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Searches for <see cref="IModReference"/>s on the file system.
    /// </summary>
    public class FileSystemModFinder : IModReferenceFinder
    {
        /// <summary>
        /// Searches mods for the given <paramref name="game"/>.
        /// </summary>
        /// <param name="game">The game which hosts the mods.</param>
        /// <returns>A set of <see cref="IModReference"/>s.
        /// The <see cref="IModReference.Identifier"/> either holds the absolute path or Steam Workshops ID.</returns>
        /// <exception cref="GameException">If the <paramref name="game"/> is not installed.</exception>
        public ISet<IModReference> FindMods(IGame game)
        {
            Requires.NotNull(game, nameof(game));
            if (!game.Exists())
                throw new GameException("The game does not exist");

            var mods = new HashSet<IModReference>();
            foreach (var modReference in GetNormalMods(game).Union(GetWorkshopsMods(game)))
                mods.Add(modReference);
            return mods;
        }

        private static IEnumerable<ModReference> GetNormalMods(IGame game)
        {
            return GetAllModsFromPath(game.ModsLocation, false);
        }

        private static IEnumerable<ModReference> GetWorkshopsMods(IGame game)
        {
            return game.Platform != GamePlatform.SteamGold
                ? Enumerable.Empty<ModReference>()
                : GetAllModsFromPath(SteamGameHelpers.GetWorkshopsLocation(game), true);
        }

        private static IEnumerable<ModReference> GetAllModsFromPath(IDirectoryInfo lookupDirectory, bool isWorkshopsPath)
        {
            if (!lookupDirectory.Exists)
                yield break;

            var type = isWorkshopsPath ? ModType.Workshops : ModType.Default;
            foreach (var modDirectory in lookupDirectory.EnumerateDirectories())
            {
                var id = isWorkshopsPath
                    ? modDirectory.Name
                    : lookupDirectory.FileSystem.Path.NormalizePath(modDirectory.FullName);
                yield return new ModReference(id, type);
            }
        }
    }
}
