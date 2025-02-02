using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <summary>
/// Factory service to create game instances.
/// </summary>
public interface IGameFactory
{
    /// <summary>
    /// Creates a game instance from the specified <see cref="GameDetectionResult"/>.
    /// </summary>
    /// <remarks>
    /// Resolving the game's name is done by using a registered <see cref="IGameNameResolver"/> service.
    /// </remarks>
    /// <param name="gameDetection">The detection result information to create the game from.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <returns>The created game instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="gameDetection"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException">
    /// The game could not be created, because:
    /// <paramref name="gameDetection"/> does not represent an installed game
    /// OR
    /// the game's name could not be resolved
    /// OR
    /// the game's platform would be <see cref="GamePlatform.Undefined"/>.
    /// </exception>
    public IGame CreateGame(GameDetectionResult gameDetection, CultureInfo culture);

    /// <summary>
    /// Creates a game instance from the specified <see cref="IGameIdentity"/> and install location.
    /// </summary>
    /// <remarks>
    /// Resolving the game's name is done by using a registered <see cref="IGameNameResolver"/> service.
    /// </remarks>
    /// <param name="identity">The game identity.</param>
    /// <param name="location">The location where the game is installed.</param>
    /// <param name="checkGameExists">
    /// When set to <see langword="true"/> the method will check
    /// whether the given <paramref name="identity"/> is actually installed at <paramref name="location"/>.
    /// </param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <returns>The created game instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identity"/> or <paramref name="location"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    /// <exception cref="GameException">
    /// The game could not be created, because:
    /// the game's name could not be resolved
    /// OR
    /// the game's platform would be <see cref="GamePlatform.Undefined"/>
    /// OR
    /// the <paramref name="identity"/> does not exists at <paramref name="location"/> if <paramref name="checkGameExists"/> is <see langword="true"/>.
    /// </exception>
    public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture);

    /// <summary>
    /// Tries to create a game instance from a <see cref="GameDetectionResult"/>.
    /// </summary>
    /// <remarks>
    /// Resolving the game's name is done by using a registered <see cref="IGameNameResolver"/> service.
    /// </remarks>
    /// <param name="gameDetection">The detection result information to create the game from.</param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <param name="game">When this method returns, contains the created game instance, if created; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="gameDetection"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    public bool TryCreateGame(GameDetectionResult gameDetection, CultureInfo culture, [NotNullWhen(true)] out IGame? game);

    /// <summary>
    /// Tries to create a game instance from a given <see cref="IGameIdentity"/>.
    /// </summary>
    /// <remarks>
    /// Resolving the game's name is done by using a registered <see cref="IGameNameResolver"/> service.
    /// </remarks>
    /// <param name="identity">The game identity.</param>
    /// <param name="location">The location where the game is installed.</param>
    /// <param name="checkGameExists">
    /// When set to <see langword="true"/> the method will check
    /// whether the given <paramref name="identity"/> is actually installed at <paramref name="location"/>.
    /// </param>
    /// <param name="culture">The culture to use to resolve the game's name.</param>
    /// <param name="game">When this method returns, contains the created game instance, if created; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identity"/> or <paramref name="location"/> or <paramref name="culture"/> is <see langword="null"/>.</exception>
    public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture, [NotNullWhen(true)] out IGame? game);
}