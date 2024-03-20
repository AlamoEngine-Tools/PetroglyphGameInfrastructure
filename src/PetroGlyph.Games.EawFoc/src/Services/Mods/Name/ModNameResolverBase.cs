using System;
using System.Globalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Abstract base class for an <see cref="IModNameResolver"/>.
/// </summary>
public abstract class ModNameResolverBase : IModNameResolver
{
    /// <summary>
    /// Instance shared Service Provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// Instance shared Logger.
    /// </summary>
    protected readonly ILogger? Logger;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The Service provider.</param>
    protected ModNameResolverBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <inheritdoc/>
    public string ResolveName(IModReference modReference)
    {
        var name = ResolveName(modReference, CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(name))
        {
            var e = new PetroglyphException($"Unable to resolve the mod's name {modReference}");
            Logger?.LogError(e, e.Message);
            throw e;
        }
        return name!;
    }

    /// <inheritdoc/>
    public string? ResolveName(IModReference modReference, CultureInfo culture)
    {
        if (modReference == null)
            throw new ArgumentNullException(nameof(modReference));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        string? modName = null;
        try
        {
            modName = ResolveCore(modReference, culture);
        }
        catch (PetroglyphException ex)
        {
            Logger?.LogError(ex, ex.Message);
        }
        catch (Exception ex)
        {
            var e = new PetroglyphException($"Unable to resolve the mod's name {modReference}: {this}", ex);
            Logger?.LogError(e, e.Message);
        }
        return modName;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetType().Name;
    }

    /// <summary>
    /// Resolves an <see cref="IModReference"/>'s name with a given culture.
    /// </summary>
    /// <param name="modReference">The target <see cref="IModReference"/>.</param>
    /// <param name="culture">The target <see cref="CultureInfo"/>.</param>
    /// <returns>The resolved name or <see langword="null"/>.</returns>
    protected internal abstract string? ResolveCore(IModReference modReference, CultureInfo culture);
}