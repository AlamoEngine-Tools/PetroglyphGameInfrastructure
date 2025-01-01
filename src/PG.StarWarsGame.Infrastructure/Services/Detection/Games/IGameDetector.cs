using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Service that detects installed PG Star Wars game installations on the machine.
/// </summary>
public interface IGameDetector
{
    /// <summary>
    /// Event gets raised when a game requires initialization.
    /// </summary>
    event EventHandler<GameInitializeRequestEventArgs> InitializationRequested;

    /// <summary>
    /// Detects a game installation.
    /// </summary>
    /// <param name="gameType">The game type to detect.</param>
    /// <param name="platforms">Collection of the platforms to search for.</param>
    /// <returns>Data which holds the game's location or error information.</returns>
    /// <remarks>
    /// If <paramref name="platforms"/> is empty or contains <see cref="GamePlatform.Undefined"/> all platforms are supported.
    /// </remarks>
    GameDetectionResult Detect(GameType gameType, params ICollection<GamePlatform> platforms);

    /// <summary>
    /// Tries to detect a game installation.
    /// </summary>
    /// <param name="gameType">The game type to detect.</param>
    /// <param name="platforms">Collection of the platforms to search for.</param>
    /// <param name="result">Variable that stores the result of the operation.</param>
    /// <returns><see langword="true"/> if and only if a game location was found; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If <paramref name = "platforms" /> is empty or contains<see cref = "GamePlatform.Undefined" /> all platforms are supported.
    /// </remarks>
    bool TryDetect(GameType gameType, ICollection<GamePlatform> platforms, out GameDetectionResult result);
}