using System;
using System.Globalization;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Abstract base class for an <see cref="IModNameResolver"/>.
/// </summary>
public abstract class ModNameResolverBase : IModNameResolver
{
    /// <summary>
    /// The Service Provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger? Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModNameResolverBase"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    protected ModNameResolverBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <inheritdoc/>
    public string ResolveName(DetectedModReference detectedMod, CultureInfo culture)
    {
        if (detectedMod == null)
            throw new ArgumentNullException(nameof(detectedMod));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        if (detectedMod.ModReference.Type == ModType.Virtual)
            throw new NotSupportedException("Virtual mods are not supported.");

        Logger?.LogDebug($"Resolving name for '{detectedMod}' with culture '{culture.EnglishName}'");

        var name = detectedMod.Modinfo is not null ? detectedMod.Modinfo.Name : ResolveCore(detectedMod, culture);
        if (string.IsNullOrEmpty(name))
            Logger?.LogWarning($"Resolved null or empty name for '{detectedMod}'");
        
        return name;
    }

    /// <summary>
    /// Resolves the name of the specified mod reference and culture.
    /// </summary>
    /// <param name="detectedMod">The mod which name shall get resolved.</param>
    /// <param name="culture">The culture context.</param>
    /// <returns>The resolved name.</returns>
    protected internal abstract string ResolveCore(DetectedModReference detectedMod, CultureInfo culture);
}