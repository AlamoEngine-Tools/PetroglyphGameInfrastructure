using System;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public class SteamPetroglyphStarWarsGameDetector : GameDetector
    {
        private const uint EaWGameId = 32470;
        private const uint FocGameId = 32472;

        private readonly ISteamWrapper _steamWrapper;
        private readonly IGameRegistry? _eawGameRegistry;
        private readonly IGameRegistry? _focGameRegistry;

        public SteamPetroglyphStarWarsGameDetector(
            ISteamWrapper steamWrapper, 
            IGameRegistry? eawGameRegistry, 
            IGameRegistry? focGameRegistry, 
            IServiceProvider serviceProvider) : base(serviceProvider, true)
        {
            Requires.NotNull(steamWrapper, nameof(steamWrapper));
            _steamWrapper = steamWrapper;
            _eawGameRegistry = eawGameRegistry;
            _focGameRegistry = focGameRegistry;
        }

        protected override GameLocationData FindGameLocation(GameDetectorOptions options)
        {
            var id = options.Type switch
            {
                GameType.EaW => EaWGameId,
                GameType.Foc => FocGameId,
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };
            var registry = options.Type switch
            {
                GameType.EaW => _eawGameRegistry,
                GameType.Foc => _focGameRegistry,
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };

            if (!_steamWrapper.Installed)
                return default;

            if (!_steamWrapper.IsGameInstalled(id, out var installLocation))
                return default;

            if (registry is null)
            {
                return !GameExeExists(installLocation, options.Type)
                    ? default
                    : new GameLocationData { Location = installLocation };
            }

            if (registry.Type != options.Type)
                throw new InvalidOperationException("Incompatible registry");

            if (registry.Version is null)
            {
                Logger?.LogDebug("Registry-Key found, but games are not initialized.");
                return new GameLocationData { InitializationRequired = true };
            }

            return !GameExeExists(installLocation, options.Type)
                ? default
                : new GameLocationData { Location = installLocation };
        }
    }
}