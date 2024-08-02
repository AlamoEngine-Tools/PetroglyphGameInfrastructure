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
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <inheritdoc/>
internal class ModFactory : IModFactory
{
    private readonly Func<IDirectoryInfo, IModinfoFileFinder> _finderFunc;
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
        _finderFunc = finderFunc;
    } 

    /// <inheritdoc/>
    public IEnumerable<IPhysicalMod> FromReference(IGame game, IModReference modReference, CultureInfo culture)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game, false);
        var modinfoFinder = _finderFunc(modReferenceLocation);
        var searchResult = modinfoFinder.Find(FindOptions.FindAny);

        return !searchResult.HasVariantModinfoFiles
            ? [CreateModFromDirectoryWithTypeCheck(game, modReference, modReferenceLocation, searchResult.MainModinfo, culture)]
            : CreateVariants(game, modReference, modReferenceLocation, searchResult.Variants, culture);
    }


    /// <inheritdoc/>
    public IPhysicalMod FromReference(IGame game, IModReference modReference, IModinfo? modinfo, CultureInfo culture)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game, false);
        return CreateModFromDirectory(game, modReference, modReferenceLocation, modinfo, culture);
    }

    /// <inheritdoc/>
    public IPhysicalMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile, CultureInfo culture)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game, false);

        IModinfoFile? mainModinfoFile = null;
        if (searchModinfoFile)
        {
            var modinfoFinder = _finderFunc(modReferenceLocation);
            mainModinfoFile = modinfoFinder.Find(FindOptions.FindMain).MainModinfo;
        }

        return CreateModFromDirectoryWithTypeCheck(game, modReference, modReferenceLocation, mainModinfoFile, culture);
    }

    /// <inheritdoc/>
    public IEnumerable<IPhysicalMod> VariantsFromReference(IGame game, IModReference modReference, CultureInfo culture)
    {
        var mods = new HashSet<IPhysicalMod>();
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game, false);
        var variants = _finderFunc(modReferenceLocation).Find(FindOptions.FindVariants);
        var variantMods = CreateVariants(game, modReference, modReferenceLocation, variants, culture);
        foreach (var mod in variantMods)
        {
            if (!mods.Add(mod))
                throw new ModException(
                    mod, $"Unable to create mod {mod.Name} " +
                    $"from '{modReferenceLocation.FullName}' because it already was created within this operation.");
        }

        return mods;
    }

    /// <inheritdoc/>
    public IVirtualMod CreateVirtualVariant(IGame game, IModinfo virtualModInfo)
    {
        var mod = new VirtualMod(game, virtualModInfo, _serviceProvider);
        if (_modGameTypeResolver.TryGetGameType(ModType.Virtual, virtualModInfo, out var gameType) && gameType != game.Type)
            throw new ModException(mod, $"Unable to create mod of game type '{gameType}' for game '{game}'");
        return mod;
    }

    /// <inheritdoc/>
    public IVirtualMod CreateVirtualVariant(IGame game, string name, IList<ModDependencyEntry> dependencies, DependencyResolveLayout resolveLayout)
    {
        return new VirtualMod(name, game, dependencies, resolveLayout, _serviceProvider);
    }

    /// <inheritdoc/>
    public IVirtualMod CreateVirtualVariant(IGame game, string name, IList<IModReference> dependencies, DependencyResolveLayout resolveLayout)
    {
        var modinfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(dependencies, resolveLayout)
        };
        return CreateVirtualVariant(game, modinfo);
    }


    private IEnumerable<IPhysicalMod> CreateVariants(IGame game, IModReference modReference, 
        IDirectoryInfo modReferenceLocation, IEnumerable<IModinfoFile> variantModInfoFiles, CultureInfo culture)
    {
        var variants = new HashSet<IPhysicalMod>();
        var names = new HashSet<string>();
        foreach (var variant in variantModInfoFiles)
        {
            if (variant.FileKind == ModinfoFileKind.MainFile)
                throw new ModException(modReference, "Cannot create a variant mod from a main modinfo file.");

            var variantInfo = variant.GetModinfo();

            if (_modGameTypeResolver.TryGetGameType(modReferenceLocation, modReference.Type, variantInfo, out var gameType) && gameType != game.Type)
                continue;

            var mod = CreateModFromDirectory(game, modReference, modReferenceLocation, variantInfo, culture);
            if (!variants.Add(mod) || !names.Add(mod.Name))
                throw new ModException(
                    mod, $"Unable to create variant mod of name {mod.Name}, because it already exists");
        }

        return variants;
    }

    private IPhysicalMod CreateModFromDirectoryWithTypeCheck(IGame game, IModReference modReference, IDirectoryInfo directory, IModinfoFile? modinfoFile, CultureInfo culture)
    {
        var modInfo = modinfoFile?.GetModinfo();
        if (_modGameTypeResolver.TryGetGameType(directory, modReference.Type, modInfo, out var gameType) && gameType != game.Type)
            throw new ModException(modReference, $"Unable to create mod of game type '{gameType}' for game '{game}'");
        return CreateModFromDirectory(game, modReference, directory, modinfoFile?.GetModinfo(), culture);
    }

    private IPhysicalMod CreateModFromDirectory(IGame game, IModReference modReference, IDirectoryInfo directory, IModinfo? modinfo, CultureInfo cultureInfo)
    {
        if (modReference.Type == ModType.Virtual)
            throw new InvalidOperationException("modType cannot be a virtual mod.");
        if (!directory.Exists)
            throw new DirectoryNotFoundException($"Unable to find mod location '{directory.FullName}'");

        var isWorkshop = modReference.Type == ModType.Workshops;

        if (modinfo == null)
        {
            var name = GetModName(modReference, cultureInfo);
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
}