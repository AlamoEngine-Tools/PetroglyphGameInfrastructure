using System;
using System.Collections.Generic;
using System.Globalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// <see cref="IModNameResolver"/> which takes a sorted list of <see cref="IModNameResolver"/> and returns the first non-null or non-empty result.
/// </summary>
public sealed class CompositeModNameResolver : IModNameResolver
{
    private readonly IList<IModNameResolver> _resolvers;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeModNameResolver"/> class with a factory method for subsequent resolvers.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="resolverFactory">Factory method to create a list of <see cref="IModNameResolver"/>.</param>
    public CompositeModNameResolver(IServiceProvider serviceProvider, Func<IServiceProvider, IList<IModNameResolver>> resolverFactory)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        if (resolverFactory == null)
            throw new ArgumentNullException(nameof(resolverFactory));
        var resolvers = resolverFactory(serviceProvider);
        ThrowHelper.ThrowIfCollectionNullOrEmpty(resolvers);
        _resolvers = resolvers;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    /// <inheritdoc/>
    public string? ResolveName(IModReference modReference, CultureInfo culture)
    {
        if (modReference == null)
            throw new ArgumentNullException(nameof(modReference));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        foreach (var nameResolver in _resolvers)
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
                _logger?.LogDebug(e, $"Error while resolving mod name for '{modReference}' with resolver {nameResolver.GetType()}: {e.Message}");
                throw;
            }
        }

        _logger?.LogTrace($"Unable to resolve name for '{modReference}'");
        return null;
    }
}