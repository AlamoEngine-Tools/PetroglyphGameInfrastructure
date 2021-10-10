using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Icon
{
    /// <summary>
    /// Service to find icons for <see cref="IMod"/>s.
    /// </summary>
    public interface IModIconFinder
    {
        /// <summary>
        /// Search a given <see cref="IMod"/> for icons and returns a relative or absolute path to,
        /// or <see langword="null"/>, if no icon could be found.
        /// </summary>
        /// <param name="mod">The mod to search an icon for.</param>
        /// <returns>Relative or absolute icon path or <see langword="null"/>.</returns>
        string? FindIcon(IMod mod);
    }
}