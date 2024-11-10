using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Base implementation for a <see cref="IModLanguageFinder"/> service.
/// This class handles the modinfo spec and dependency language lookup.
/// </summary>
internal abstract class ModLanguageFinderBase : IModLanguageFinder
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
    protected static HashSet<ILanguageInfo> DefaultLanguageCollection = [LanguageInfo.Default];

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
        Helper = serviceProvider.GetRequiredService<ILanguageFinder>();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<ILanguageInfo> FindInstalledLanguages(IMod mod)
    {
        // We don't "trust" the modinfo here as the default language (EN) also gets
        // applied when nothing was specified by the mod developer.
        // Only if we have more than the default language, we trust the modinfo.
        var modinfo = mod.ModInfo;
        if (modinfo is not null && !IsEmptyOrDefault(modinfo.Languages))
            return modinfo.Languages;


        var foundLanguages = FindInstalledLanguagesCore(mod);
        if (!IsEmptyOrDefault(foundLanguages) && !_lookupInheritedLanguages && mod.DependencyResolveStatus != DependencyResolveStatus.Resolved)
            return foundLanguages;

        return GetInheritedLanguages(mod, foundLanguages);
    }

    /// <summary>
    /// Checks whether a given set of <see cref="ILanguageInfo"/> is empty or only contains the default value.
    /// Returns <see langword="true"/> in this case; <see langword="false"/> otherwise.
    /// </summary>
    /// <param name="languages">The enumeration to lookup.</param>
    protected static bool IsEmptyOrDefault(IReadOnlyCollection<ILanguageInfo> languages)
    {
        if (languages.Count == 0)
            return true;
        if (languages.Count == 1 && languages.First().Equals(LanguageInfo.Default))
            return true;
        return false;
    }

    private static IReadOnlyCollection<ILanguageInfo> GetInheritedLanguages(IMod mod, IReadOnlyCollection<ILanguageInfo> defaultLanguages)
    {
        foreach (var dependency in mod.Dependencies)
        {
            var dependencyLanguages = dependency.Mod.InstalledLanguages;
            if (!IsEmptyOrDefault(dependencyLanguages))
                return dependencyLanguages;
        }
        return defaultLanguages;
    }

    /// <summary>
    /// Implementation specific logic to fetch the installed languages of the <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">The target <see cref="IMod"/>.</param>
    /// <returns>A set of fetched <see cref="ILanguageInfo"/>s.</returns>
    protected internal abstract IReadOnlyCollection<ILanguageInfo> FindInstalledLanguagesCore(IMod mod);
}