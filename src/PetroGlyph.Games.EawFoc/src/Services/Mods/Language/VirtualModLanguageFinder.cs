using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    /// <summary>
    /// Searches installed languages for non-virtual mod dependencies.
    /// <remarks>Dependency Resolving should already be performed. Otherwise only ENGLISH - FullLocalized will be returned.</remarks> 
    /// </summary>
    public class VirtualModLanguageFinder : ModLanguageFinderBase
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public VirtualModLanguageFinder(IServiceProvider serviceProvider) : base(serviceProvider, true)
        {
        }

        /// <inheritdoc/>
        protected internal override ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod)
        {
            if (mod.Type != ModType.Virtual)
                throw new NotSupportedException($"Mod type: {mod.Type} is not supported by this instance.");

            // Since virtual mods inherit their language from a (physical) dependency.
            // There is nothing to do here.
            // Simply returning the default collection. The base class will do the rest.
            return DefaultLanguageCollection;
        }
    }
}