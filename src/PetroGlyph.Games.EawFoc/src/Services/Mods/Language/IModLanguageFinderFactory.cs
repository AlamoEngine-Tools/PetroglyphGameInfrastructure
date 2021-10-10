using System;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    /// <summary>
    /// Factory service to create the correct <see cref="IModLanguageFinder"/> for a given <see cref="IMod"/>.
    /// </summary>
    public interface IModLanguageFinderFactory
    {
        /// <summary>
        /// Create the correct <see cref="IModLanguageFinder"/>.
        /// </summary>
        /// <param name="mod">The target mod.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The <see cref="IModLanguageFinder"/> instance.</returns>
        IModLanguageFinder CreateLanguageFinder(IMod mod, IServiceProvider serviceProvider);
    }
}