using System;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Default factory implementation which returns separate <see cref="IModLanguageFinder"/> for Virtual and Physical Mods.
/// </summary>
internal class ModLanguageFinderFactory(IServiceProvider serviceProvider) : IModLanguageFinderFactory
{
    /// <inheritdoc/>
    public IModLanguageFinder CreateLanguageFinder(IMod mod)
    {
        if (mod.Type == ModType.Virtual)
            return new VirtualModLanguageFinder(serviceProvider);
        return new PhysicalModLanguageFinder(serviceProvider, true);
    }
}