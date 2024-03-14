using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;

namespace PetroGlyph.Games.EawFoc.Services.Language;

/// <summary>
/// Base implementation for a <see cref="IModLanguageFinder"/> service.
/// This class handles the modinfo spec and dependency language lookup.
/// </summary>
public abstract class ModLanguageFinderBase : IModLanguageFinder
{
    /// <summary>
    /// Shared service provider instance.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// Shared <see cref="IModLanguageFinder"/> instance.
    /// </summary>
    protected readonly ILanguageFinder Helper;

    /// <summary>
    /// Default return result containing only  ENGLISH - FullLocalized.
    /// </summary>
    protected static ISet<ILanguageInfo> DefaultLanguageCollection =
        new HashSet<ILanguageInfo> { LanguageInfo.Default };

    private readonly bool _lookupInheritedLanguages;


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="lookupInheritedLanguages">When set to <see langword="true"/>the target mod's dependency
    /// languages will also be considered if, and only if, the target mod would only return ENGLISH - FullLocalized.</param>
    /// <remarks>When <paramref name="lookupInheritedLanguages"/> is set to <see langword="true"/>,
    /// dependency Resolving should already be performed. Otherwise the <paramref name="lookupInheritedLanguages"/> has no effect.</remarks> 
    protected ModLanguageFinderBase(IServiceProvider serviceProvider, bool lookupInheritedLanguages)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _lookupInheritedLanguages = lookupInheritedLanguages;
        Helper = serviceProvider.GetService<ILanguageFinder>() ?? new FileBasedLanguageFinder();
    }

    /// <inheritdoc/>
    public virtual ISet<ILanguageInfo> FindInstalledLanguages(IMod mod)
    {
        // We don't "trust" the modinfo here as the default language (EN) also gets
        // applied when nothing was specified by the mod developer.
        // Only if we have more than the default language, we trust the modinfo.
        var modinfo = mod.ModInfo;
        if (modinfo is not null && !IsEmptyOrDefault(modinfo.Languages))
            return new HashSet<ILanguageInfo>(modinfo.Languages);


        var foundLanguages = FindInstalledLanguagesCore(mod);
        if (!IsEmptyOrDefault(foundLanguages))
            return foundLanguages;

        if (!_lookupInheritedLanguages)
            return DefaultLanguageCollection;

        var inheritedLanguages = GetInheritedLanguages(mod);
        return !inheritedLanguages.Any() ? DefaultLanguageCollection : inheritedLanguages;
    }

    /// <summary>
    /// Checks whether a given set of <see cref="ILanguageInfo"/> is empty or only contains the default value.
    /// Returns <see langword="true"/> in this case; <see langword="false"/> otherwise.
    /// </summary>
    /// <param name="languages">The enumeration to lookup.</param>
    protected static bool IsEmptyOrDefault(IEnumerable<ILanguageInfo> languages)
    {
        var languageInfos = languages.ToList();
        return !languageInfos.Any() || languageInfos.All(x => x.Equals(LanguageInfo.Default));
    }

    /// <summary>
    /// Gets installed languages of (already resolved) dependencies.
    /// </summary>
    /// <param name="mod">The target <see cref="IMod"/>.</param>
    /// <returns>Set of installed languages of a dependency.</returns>
    /// <remarks>This implementation is not greedy, meaning it returns the first non-<see cref="IsEmptyOrDefault"/> result.</remarks>
    protected virtual ISet<ILanguageInfo> GetInheritedLanguages(IMod mod)
    {
        if (mod.DependencyResolveStatus != DependencyResolveStatus.Resolved)
            return DefaultLanguageCollection;

        foreach (var dependency in mod.Dependencies)
        {
            var dependencyLanguages = dependency.Mod.InstalledLanguages;
            if (!IsEmptyOrDefault(dependencyLanguages))
                return dependencyLanguages;
        }
        return new HashSet<ILanguageInfo>();
    }

    /// <summary>
    /// Implementation specific logic to fetch the installed languages of the <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">The target <see cref="IMod"/>.</param>
    /// <returns>A set of fetched <see cref="ILanguageInfo"/>s.</returns>
    protected internal abstract ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod);
}