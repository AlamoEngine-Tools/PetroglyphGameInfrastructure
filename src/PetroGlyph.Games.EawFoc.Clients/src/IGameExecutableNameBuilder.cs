using PetroGlyph.Games.EawFoc.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Service that gets the correct executable file name for a Petroglyph Star Wars game.
/// </summary>
public interface IGameExecutableNameBuilder
{
    /// <summary>
    /// Converts <paramref name="game"/> and <paramref name="buildType"/> into the correct executable file name.
    /// </summary>
    /// <param name="game">The game to get the executable name from.</param>
    /// <param name="buildType">The game's build type.</param>
    /// <returns>The executable file name.</returns>
    string GetExecutableFileName(IGame game, GameBuildType buildType);
}