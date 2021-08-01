using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;

namespace PetroGlyph.Games.EawFoc.Services
{
    /// <summary>
    /// Factory service to create game instances.
    /// </summary>
    public interface IGameFactory
    {
        /// <summary>
        /// Tries to create a game instance from a <see cref="GameDetectionResult"/>.
        /// </summary>
        /// <param name="gameDetection">The detection result information.</param>
        /// <param name="game">The created game instance. <see langword="null"/> if not instance could be created.</param>
        /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
        public bool TryCreateGame(GameDetectionResult gameDetection, out IGame? game);

        /// <summary>
        /// Creates a game instance from a <see cref="GameDetectionResult"/>.
        /// </summary>
        /// <param name="gameDetection">The detection result information.</param>
        /// <returns>The created game instance.</returns>
        /// <exception cref="GameException">If the instance could not be created.</exception>
        public IGame CreateGame(GameDetectionResult gameDetection);

        /// <summary>
        /// Tries to create a game instance from a given <see cref="IGameIdentity"/>.
        /// </summary>
        /// <param name="identity">The result information.</param>
        /// <param name="location">The location where the game is installed.</param>
        /// <param name="checkGameExists">When set to <see langword="true"/>a check will be performed if the given <paramref name="identity"/> is really installed at the the <paramref name="location"/></param>
        /// <param name="game">The created game instance. <see langword="null"/> if not instance could be created.</param>
        /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, out IGame? game);

        /// <summary>
        /// Creates a game instance from a given <see cref="IGameIdentity"/>.
        /// </summary>
        /// <param name="identity">The result information.</param>
        /// <param name="location">The location where the game is installed.</param>
        /// <param name="checkGameExists">When set to <see langword="true"/>a check will be performed if the given <paramref name="identity"/> is really installed at the the <paramref name="location"/></param>
        /// <returns>The created game instance.</returns>
        /// <exception cref="GameException">If the instance could not be created.</exception>
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists);

        /// <summary>
        /// Tries to create a game instance from a <see cref="GameDetectionResult"/> and checks whether that instance exists.
        /// </summary>
        /// <param name="identity">The result information.</param>
        /// <param name="location">The location where the game is installed.</param>
        /// <param name="game">The created game instance. <see langword="null"/> if not instance could be created.</param>
        /// <returns><see langword="true"/>if creation was successful; <see langword="false"/> otherwise.</returns>
        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, out IGame? game);

        /// <summary>
        /// Creates a game instance from a given <see cref="IGameIdentity"/> and checks whether that instance exists.
        /// </summary>
        /// <param name="identity">The result information.</param>
        /// <param name="location">The location where the game is installed.</param>
        /// <returns>The created game instance.</returns>
        /// <exception cref="GameException">If the instance could not be created.</exception>
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location);
    }
}
