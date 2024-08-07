﻿using System;
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
    public string? ResolveName(IModReference modReference, CultureInfo culture)
    {
        if (modReference == null)
            throw new ArgumentNullException(nameof(modReference));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        try
        {
            var name = ResolveCore(modReference, culture);
            if (string.IsNullOrEmpty(name))
                Logger?.LogTrace($"Resolver '{this}' resolved null or empty name for '{modReference}'.");
            return name;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Resolver '{this}' had an error while resolving the name for {modReference}: {ex.Message}");
            throw;
        }
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