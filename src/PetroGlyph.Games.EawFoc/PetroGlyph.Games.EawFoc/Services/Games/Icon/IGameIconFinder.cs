using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Icon
{
    /// <summary>
    /// Service to find the game's icon.
    /// </summary>
    public interface IGameIconFinder
    {
        /// <summary>
        /// Gets and absolute or relative path (relative to the game's root directory) of the icon file of the game.
        /// Return <see langword="null"/> if no icon file was found.
        /// </summary>
        string? FindIcon(IGame game);
    }
}