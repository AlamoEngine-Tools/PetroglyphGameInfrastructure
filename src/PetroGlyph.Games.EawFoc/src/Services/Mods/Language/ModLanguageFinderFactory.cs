using System;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    /// <summary>
    /// Default factory implementation which returns separate <see cref="IModLanguageFinder"/> for Virtual and Physical Mods.
    /// </summary>
    public class ModLanguageFinderFactory : IModLanguageFinderFactory
    {
        /// <inheritdoc/>
        public IModLanguageFinder CreateLanguageFinder(IMod mod, IServiceProvider serviceProvider)
        {
            if (mod.Type == ModType.Virtual)
                return new VirtualModLanguageFinder(serviceProvider);
            return new PhysicalModLanguageFinder(serviceProvider, true);
        }
    }
}