using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Games.Registry;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Searches a the current machine for a Petroglyph Star Wars Game by the most commonly wanted search heuristic.
    /// 1. Searches the current directory of the running process for an game installation
    /// 2. Searches the Registry for a game installation
    /// </summary>
    public static class DefaultGameDetectorFactory
    {
        /// <summary>
        /// Creates the detector instance.
        /// </summary>
        /// <param name="gameRegistry">The game's registry which shall be used.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public static IGameDetector CreateDefaultDetector(IGameRegistry gameRegistry, IServiceProvider serviceProvider)
        {
            Requires.NotNull(gameRegistry, nameof(gameRegistry));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            var currentDirDetector = DirectoryGameDetector.CurrentDirectoryGameDetector(serviceProvider);
            var registryDetector = new RegistryGameDetector(gameRegistry, true, serviceProvider);
            return new CompositeGameDetector(new List<IGameDetector> { currentDirDetector, registryDetector }, serviceProvider);
        }
    }
}