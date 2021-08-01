using System;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games.Registry;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public sealed class RegistryGameDetector : GameDetector, IDisposable
    {
        private readonly IGameRegistry _registry;

        public RegistryGameDetector(IGameRegistry registry, bool tryHandleInitialization, IServiceProvider serviceProvider)
            : base(serviceProvider, tryHandleInitialization)
        {
            Requires.NotNull(registry, nameof(registry));
            _registry = registry;
        }

        protected internal override GameLocationData FindGameLocation(GameDetectorOptions options)
        {
            Logger?.LogDebug("Attempting to fetch the game from the registry.");
            if (!_registry.Exits)
            {
                Logger?.LogDebug("The Game's Registry does not exist.");
                return default;
            }

            if (_registry.Version is null)
            {
                Logger?.LogDebug("Registry-Key found, but games are not initialized.");
                return new GameLocationData { InitializationRequired = true };
            }

            var exeDirectory = _registry.ExePath?.Directory;
            if (exeDirectory is not null)
                return new GameLocationData { Location = exeDirectory };

            Logger?.LogDebug("Could not get instal location from registry path.");
            return default;
        }

        public void Dispose()
        {
            _registry.Dispose();
        }
    }
}
