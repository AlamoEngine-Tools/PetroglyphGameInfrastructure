using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using EawModinfo.File;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <inheritdoc/>
internal class ModFactory : IModFactory
{
    private readonly IModReferenceLocationResolver _referenceLocationResolver;
    private readonly IModNameResolver _nameResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly IModGameTypeResolver _modGameTypeResolver;

    /// <summary>
    /// Creates a new mod factory.
    /// </summary>
    /// <param name="serviceProvider">Service provider which gets passed to an <see cref="IMod"/> instance.</param>
    public ModFactory(IServiceProvider serviceProvider) : this(serviceProvider, DefaultModinfoFileFinder)
    {
    }

    internal ModFactory(IServiceProvider serviceProvider, Func<IDirectoryInfo, IModinfoFileFinder> finderFunc)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _referenceLocationResolver = serviceProvider.GetRequiredService<IModReferenceLocationResolver>();
        _nameResolver = serviceProvider.GetRequiredService<IModNameResolver>();
        _modGameTypeResolver = serviceProvider.GetRequiredService<IModGameTypeResolver>();
    }

    /// <inheritdoc/>
    public IMod FromReference(IGame game, DetectedModReference modReference, CultureInfo culture)
    {
        return FromReference(game, modReference.ModReference, modReference.ModInfo, culture);
    }

    /// <inheritdoc/>
    public IMod FromReference(IGame game, IModReference modReference, IModinfo? modinfo, CultureInfo culture)
    {
        if (modReference.Type == ModType.Virtual)
        {
            if (modinfo is null)
                throw new ModException(modReference, "modinfo cannot be null for creating virtual mods.");
            return CreateVirtualMod(game, modinfo);
        }

        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game);
        return CreateModFromDirectoryWithTypeCheck(game, modReference, modReferenceLocation, modinfo, culture);
    }

    /// <inheritdoc/>
    public IVirtualMod CreateVirtualMod(IGame game, IModinfo virtualModInfo)
    {
        if (virtualModInfo == null) 
            throw new ArgumentNullException(nameof(virtualModInfo));
        
        var mod = new VirtualMod(game, virtualModInfo, _serviceProvider);

        ThrowIfDefinitelyNotGameType(game.Type, virtualModInfo, mod);

        return mod;
    }

    /// <inheritdoc/>
    public IVirtualMod CreateVirtualMod(IGame game, string name, IList<ModDependencyEntry> dependencies, DependencyResolveLayout resolveLayout)
    {
        return new VirtualMod(name, game, dependencies, resolveLayout, _serviceProvider);
    }

    private Mod CreateModFromDirectoryWithTypeCheck(IGame game, IModReference modReference, IDirectoryInfo directory, IModinfo? modinfo, CultureInfo culture)
    {
        if (modReference.Type == ModType.Virtual)
            throw new InvalidOperationException("modType cannot be a virtual mod.");
        if (!directory.Exists)
            throw new DirectoryNotFoundException($"Unable to find mod location '{directory.FullName}'");

        // We don't trust the modtype of the modReference.
        ThrowIfDefinitelyNotGameType(game.Type, directory, modReference.Type, modinfo, modReference);

        var isWorkshop = modReference.Type == ModType.Workshops;

        if (modinfo == null)
        {
            var name = GetModName(modReference, culture);
            return new Mod(game, directory, isWorkshop, name, _serviceProvider);
        }

        modinfo.Validate();
        return new Mod(game, directory, isWorkshop, modinfo, _serviceProvider);
    }

    private string GetModName(IModReference modReference, CultureInfo culture)
    {
        var name = _nameResolver.ResolveName(modReference, culture);
        if (string.IsNullOrEmpty(name))
            throw new ModException(modReference, "Unable to create a mod with an empty name.");
        return name!;
    }

    private static IModinfoFileFinder DefaultModinfoFileFinder(IDirectoryInfo dir)
    {
        return new ModinfoFileFinder(dir);
    }

    private void ThrowIfDefinitelyNotGameType(GameType expected, IModinfo modinfo, IModReference mod)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        if (_modGameTypeResolver.TryGetGameType(modinfo, out var modGameType) && expected != modGameType)
            throw new ModException(mod, $"Unable to create mod of game type '{modGameType}' for game '{expected}'");
    }

    private void ThrowIfDefinitelyNotGameType(GameType expected, IDirectoryInfo modDirectory, ModType modType, IModinfo? modinfo, IModReference mod)
    {
        // If the type resolver was unable to find the type, we have to assume that the current mod matches to the game.
        // Otherwise, we'd produce false negatives. Only if the resolver was able to determine a result, we use that finding.
        if (_modGameTypeResolver.TryGetGameType(modDirectory, modType, modinfo, out var modGameType) && expected != modGameType)
            throw new ModException(mod, $"Unable to create mod of game type '{modGameType}' for game '{expected}'");
    }
}