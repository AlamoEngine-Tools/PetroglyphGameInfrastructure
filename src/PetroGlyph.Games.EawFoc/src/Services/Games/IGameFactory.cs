using System.Globalization;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <summary>
/// Factory service to create game instances.
/// </summary>
public interface IGameFactory
{ 
    /// <summary>
    /// Creates a game instance from a <see cref="GameDetectionResult"/>.
    /// </summary>
    /// <param name="gameDetection">The detection result information.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <returns>The created game instance.</returns>
    /// <exception cref="GameException">If the instance could not be created.</exception>
    public IGame CreateGame(GameDetectionResult gameDetection, CultureInfo culture);

    /// <summary>
    /// Creates a game instance from a given <see cref="IGameIdentity"/>.
    /// </summary>
    /// <param name="identity">The result information.</param>
    /// <param name="location">The location where the game is installed.</param>
    /// <param name="checkGameExists">When set to <see langword="true"/>a check will be performed if the given <paramref name="identity"/> is really installed at the <paramref name="location"/>.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <returns>The created game instance.</returns>
    /// <exception cref="GameException">If the instance could not be created.</exception>
    public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture);

    /// <summary>
    /// Tries to create a game instance from a <see cref="GameDetectionResult"/>.
    /// </summary>
    /// <param name="gameDetection">The detection result information.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <param name="game">The created game instance. <see langword="null"/> if not instance could be created.</param>
    /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
    public bool TryCreateGame(GameDetectionResult gameDetection, CultureInfo culture, out IGame? game);

    /// <summary>
    /// Tries to create a game instance from a given <see cref="IGameIdentity"/>.
    /// </summary>
    /// <param name="identity">The result information.</param>
    /// <param name="location">The location where the game is installed.</param>
    /// <param name="checkGameExists">When set to <see langword="true"/>a check will be performed if the given <paramref name="identity"/> is really installed at the <paramref name="location"/>.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <param name="game">The created game instance. <see langword="null"/> if not instance could be created.</param>
    /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
    public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture, out IGame? game);
}