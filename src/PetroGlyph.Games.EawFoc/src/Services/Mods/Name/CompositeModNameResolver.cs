using System;
using System.Collections.Generic;
using System.Globalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// <see cref="IModNameResolver"/> which takes a sorted list of <see cref="IModNameResolver"/> and returns the first valid result.
/// </summary>
public sealed class CompositeModNameResolver : IModNameResolver
{
    private readonly IList<IModNameResolver> _sortedResolvers;
    private readonly ILogger? _logger;

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <param name="sortedResolvers">Prioritized list of <see cref="IModNameResolver"/>.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public CompositeModNameResolver(IList<IModNameResolver> sortedResolvers, IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        ThrowHelper.ThrowIfCollectionNullOrEmpty(sortedResolvers);
        _sortedResolvers = sortedResolvers;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <summary>
    /// Create a new <see cref="IModNameResolver"/> which resolves the name from:
    /// <br/>
    /// 1. The mod's Steam Workshops page (if applicable)
    /// <br/>
    /// 2. The mod's directory name.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IModNameResolver CreateDefaultModNameResolver(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        var resolvers = new List<IModNameResolver>
        {
            new OnlineWorkshopNameResolver(serviceProvider),
            new DirectoryModNameResolver(serviceProvider)
        };
        return new CompositeModNameResolver(resolvers, serviceProvider);
    }

    /// <inheritdoc/>
    public string ResolveName(IModReference modReference)
    {
        var name = ResolveName(modReference, CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(name))
        {
            var e = new PetroglyphException($"Unable to resolve the mod's name {modReference}");
            _logger?.LogError(e, e.Message);
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

        foreach (var nameResolver in _sortedResolvers)
        {
            try
            {
                try
                {
                    _logger?.LogDebug($"Resolving mod name with {nameResolver}");
                    var name = nameResolver.ResolveName(modReference, culture);
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
                catch (Exception e)
                {
                    throw new PetroglyphException("Error while resolving a mod's name", e);
                }
            }
            catch (PetroglyphException e)
            {
                _logger?.LogDebug(e, e.Message);
            }
        }
        return null;
    }
}