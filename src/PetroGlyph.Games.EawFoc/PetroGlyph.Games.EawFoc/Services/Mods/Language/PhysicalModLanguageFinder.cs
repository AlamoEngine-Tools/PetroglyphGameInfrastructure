using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    /// <summary>
    /// Searches the mod's files for evidences of installed languages.
    /// </summary>
    public class PhysicalModLanguageFinder : ModLanguageFinderBase
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="lookupInheritedLanguages">When set to <see langword="true"/>the target mod's dependency
        /// languages will also be considered if, and only if, the target mod would only return ENGLISH - FullLocalized.</param>
        /// <remarks>When <paramref name="lookupInheritedLanguages"/> is set to <see langword="true"/>,
        /// dependency Resolving should already be performed. Otherwise the <paramref name="lookupInheritedLanguages"/> has no effect.</remarks> 
        public PhysicalModLanguageFinder(IServiceProvider serviceProvider, bool lookupInheritedLanguages)
            : base(serviceProvider, lookupInheritedLanguages)
        {
        }

        /// <inheritdoc/>
        protected internal override ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod)
        {
            if (mod is not IPhysicalMod physicalMod)
                throw new NotSupportedException("Non physical mod is not supported by this instance.");
            var text = Helper.GetTextLocalizations(physicalMod);
            var speechMeg = Helper.GetSpeechLocalizationsFromMegs(physicalMod);
            var speechFolder = Helper.GetSpeechLocalizationsFromFolder(physicalMod);
            var sfx = Helper.GetSfxMegLocalizations(physicalMod);
            return Helper.Merge(text, speechFolder, speechMeg, sfx);
        }
    }
}