using System;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Service that detects installed PG Star Wars game installations on this machine.
    /// </summary>
    public interface IGameDetector
    {
        /// <summary>
        /// Event gets raised when a game requires initialization.
        /// </summary>
        event EventHandler<GameInitializeRequestEventArgs> InitializationRequested;

        /// <summary>
        /// Detects a game instance.
        /// </summary>
        /// <param name="options">Provided search options.</param>
        /// <returns>Data which holds the game's location or error information.</returns>
        GameDetectionResult Detect(GameDetectorOptions options);

        /// <summary>
        /// Detects a game instance.
        /// </summary>
        /// <param name="options">Provided search options.</param>
        /// <param name="result">Data which holds the game's location or error information.</param>
        /// <returns><see langword="true"/> when a game was found; <see langword="false"/> otherwise.</returns>
        bool TryDetect(GameDetectorOptions options, out GameDetectionResult result);
    }
}