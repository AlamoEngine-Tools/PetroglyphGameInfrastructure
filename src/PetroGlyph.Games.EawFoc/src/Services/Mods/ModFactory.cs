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
public class ModFactory : IModFactory
{
    private static readonly Func<IDirectoryInfo, IModinfoFileFinder> DefaultModinfoFileFinderFactory =
        ModinfoFileFinderFactory;

    private static IModinfoFileFinder ModinfoFileFinderFactory(IDirectoryInfo directory)
    {
        return new ModinfoFileFinder(directory);
    }

    private readonly IModReferenceLocationResolver _referenceLocationResolver;
    private readonly Func<IDirectoryInfo, IModinfoFileFinder> _defaultModinfoFileFinderFactory;
    private readonly IModNameResolver _nameResolver;
    private readonly CultureInfo _culture;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new mod factory.
    /// </summary>
    /// <param name="serviceProvider">Service provider which gets passed to an <see cref="IMod"/> instance.</param>
    public ModFactory(IServiceProvider serviceProvider)
        : this(DefaultModinfoFileFinderFactory, CultureInfo.InvariantCulture, serviceProvider)
    {
    }

    /// <summary>
    /// Creates a new mod factory.
    /// </summary>
    /// <param name="culture">The culture which shall get used to resolve a mod's name.</param>
    /// <param name="serviceProvider">Service provider which gets passed to an <see cref="IMod"/> instance.</param>
    public ModFactory(CultureInfo culture, IServiceProvider serviceProvider)
        : this(DefaultModinfoFileFinderFactory, culture, serviceProvider)
    {
    }

    internal ModFactory(
        Func<IDirectoryInfo, IModinfoFileFinder> defaultModinfoFileFinderFactory,
        CultureInfo culture,
        IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        _referenceLocationResolver = serviceProvider.GetRequiredService<IModReferenceLocationResolver>();
        _defaultModinfoFileFinderFactory = defaultModinfoFileFinderFactory ?? throw new ArgumentNullException(nameof(defaultModinfoFileFinderFactory));
        _nameResolver = serviceProvider.GetRequiredService<IModNameResolver>();
        _culture = culture ?? throw new ArgumentNullException(nameof(culture));
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IEnumerable<IPhysicalMod> FromReference(IGame game, IModReference modReference)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game);
        var modinfoFinder = _defaultModinfoFileFinderFactory(modReferenceLocation);
        var searchResult = modinfoFinder.Find(FindOptions.FindAny);

        return !searchResult.HasVariantModinfoFiles
            ? new[] { CreateModFromDirectory(game, modReference, modReferenceLocation, searchResult.MainModinfo) }
            : CreateVariants(game, modReference, modReferenceLocation, searchResult.Variants);
    }


    /// <inheritdoc/>
    public IPhysicalMod FromReference(IGame game, IModReference modReference, IModinfo? modinfo)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game);
        return CreateModFromDirectory(game, modReference, modReferenceLocation, modinfo);
    }

    /// <inheritdoc/>
    public IPhysicalMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile)
    {
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game);

        IModinfoFile? mainModinfoFile = null;
        if (searchModinfoFile)
        {
            var modinfoFinder = _defaultModinfoFileFinderFactory(modReferenceLocation);
            mainModinfoFile = modinfoFinder.Find(FindOptions.FindMain).MainModinfo;
        }

        return CreateModFromDirectory(game, modReference, modReferenceLocation, mainModinfoFile);
    }

    /// <inheritdoc/>
    public IEnumerable<IPhysicalMod> VariantsFromReference(IGame game, IModReference modReference)
    {
        var mods = new HashSet<IPhysicalMod>();
        var modReferenceLocation = _referenceLocationResolver.ResolveLocation(modReference, game);
        var variants = _defaultModinfoFileFinderFactory(modReferenceLocation).Find(FindOptions.FindVariants);
        var variantMods = CreateVariants(game, modReference, modReferenceLocation, variants);
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
        return new VirtualMod(game, virtualModInfo, _serviceProvider);
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
            { Dependencies = new DependencyList(dependencies, resolveLayout) };
        return CreateVirtualVariant(game, modinfo);
    }


    private IEnumerable<IPhysicalMod> CreateVariants(IGame game, IModReference modReference,
        IDirectoryInfo modReferenceLocation, IEnumerable<IModinfoFile> variantModInfoFiles)
    {
        var variants = new HashSet<IPhysicalMod>();
        var names = new HashSet<string>();
        foreach (var variant in variantModInfoFiles)
        {
            if (variant.FileKind == ModinfoFileKind.MainFile)
                throw new ModException(modReference, "Cannot create a variant mod from a main modinfo file.");

            var mod = CreateModFromDirectory(game, modReference, modReferenceLocation, variant);
            if (!variants.Add(mod) || !names.Add(mod.Name))
                throw new ModException(
                    mod, $"Unable to create variant mod of name {mod.Name}, because it already exists");
        }

        return variants;
    }

    private IPhysicalMod CreateModFromDirectory(IGame game, IModReference modReference, IDirectoryInfo directory,
        IModinfoFile? modinfoFile)
    {
        return CreateModFromDirectory(game, modReference, directory, modinfoFile?.GetModinfo());
    }

    private IPhysicalMod CreateModFromDirectory(IGame game, IModReference modReference, IDirectoryInfo directory,
        IModinfo? modinfo)
    {
        if (modReference.Type == ModType.Virtual)
            throw new InvalidOperationException("modType cannot be a virtual mod.");
        if (!directory.Exists)
            throw new DirectoryNotFoundException($"Unable to find mod location '{directory.FullName}'");

        if (modinfo == null)
        {
            var name = GetModName(modReference);
            return new Mod(game, directory, modReference.Type == ModType.Workshops, name, _serviceProvider);
        }

        modinfo.Validate();
        return new Mod(game, directory, modReference.Type == ModType.Workshops, modinfo, _serviceProvider);
    }

    private string GetModName(IModReference modReference)
    {
        var name = _nameResolver.ResolveName(modReference, _culture);
        if (string.IsNullOrEmpty(name))
            name = _nameResolver.ResolveName(modReference);
        if (string.IsNullOrEmpty(name))
            throw new ModException(modReference, "Unable to create a mod with an empty name.");
        return name!;
    }
}