using System;
using System.Globalization;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services
{
    /// <inheritdoc/>
    public class GameFactory : IGameFactory
    {
        private static readonly IGameNameResolver FallbackNameResolver = new EnglishGameNameResolver();
        private readonly IGameNameResolver _nameResolver;
        private readonly CultureInfo _nameCulture;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new factory instances. Uses <see cref="EnglishGameNameResolver"/> for the game's name resolution.
        /// </summary>
        /// <param name="serviceProvider">The service provider which gets passed to the game instances.</param>
        public GameFactory(IServiceProvider serviceProvider)
            : this(FallbackNameResolver, CultureInfo.InvariantCulture, serviceProvider)
        {
        }

        /// <summary>
        /// Creates a new factory instances. Uses <see cref="CultureInfo.InvariantCulture"/> to resolve the game's name.
        /// </summary>
        /// <param name="nameResolver"><see cref="IGameNameResolver"/> to get the game's name.</param>
        /// <param name="serviceProvider">The service provider which gets passed to the game instances.</param>
        public GameFactory(IGameNameResolver nameResolver, IServiceProvider serviceProvider) : this(nameResolver, CultureInfo.InvariantCulture, serviceProvider)
        {
        }

        /// <summary>
        /// Creates a new factory instances.
        /// </summary>
        /// <param name="nameResolver"><see cref="IGameNameResolver"/> to get the game's name.</param>
        /// <param name="nameCulture">The culture information to resolve the game's name.</param>
        /// <param name="serviceProvider">The service provider which gets passed to the game instances.</param>
        public GameFactory(IGameNameResolver nameResolver, CultureInfo nameCulture, IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(nameResolver, nameof(nameResolver));
            Requires.NotNull(nameCulture, nameof(nameCulture));
            _nameResolver = nameResolver;
            _nameCulture = nameCulture;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IGame CreateGame(GameDetectionResult gameDetection)
        {
            Requires.NotNull(gameDetection, nameof(gameDetection));
            if (gameDetection.Error is not null)
                throw new GameException("Unable to create a game from faulted detection result.");
            if (gameDetection.GameLocation is null)
                throw new GameException($"Unable to create game {gameDetection.GameIdentity.Type}, because it's not installed on this machine");
            return CreateGame(gameDetection.GameIdentity, gameDetection.GameLocation, false);
        }

        /// <inheritdoc/>
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists)
        {
            Requires.NotNull(location, nameof(location));
            if (identity.Platform == GamePlatform.Undefined)
                throw new ArgumentException("Cannot create a game with undefined platform");

            var name = _nameResolver.ResolveName(identity, _nameCulture) ?? FallbackNameResolver.ResolveName(identity);
            var game = new PetroglyphStarWarsGame(identity, location, name, _serviceProvider);
            if (checkGameExists && !game.Exists())
                throw new GameException($"Game does not exists at location: {location}");

            return game;
        }

        /// <inheritdoc/>
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location)
        {
            var game = CreateGame(identity, location, true);
            return game;
        }

        /// <inheritdoc/>
        public bool TryCreateGame(GameDetectionResult gameDetection, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(gameDetection);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(identity, location, checkGameExists);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(identity, location);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}