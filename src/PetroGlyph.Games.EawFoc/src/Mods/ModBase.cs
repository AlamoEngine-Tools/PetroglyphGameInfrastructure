using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Spec.Equality;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Services.Language;
using Semver;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Base implementation for Mods
/// </summary>
public abstract class ModBase : PlayableModContainer, IMod
{
    /// <inheritdoc/>
    public event EventHandler<ResolvingModinfoEventArgs>? ResolvingModinfo;
    /// <inheritdoc/>
    public event EventHandler<ModinfoResolvedEventArgs>? ModinfoResolved;
    /// <inheritdoc/>
    public event EventHandler<ModDependenciesChangedEventArgs>? DependenciesChanged;

    private SemVersion? _modVersion;
    private IModinfo? _modInfo;
    private bool _modinfoSearched;

    /// <summary>
    /// Shared, internal, mutable, dependency list which gets used for <see cref="Dependencies"/>.
    /// </summary>
    protected readonly List<ModDependencyEntry> DependenciesInternal = new();

    /// <inheritdoc/>
    public abstract string Identifier { get; }

    /// <inheritdoc/>
    public override IGame Game { get; }

    /// <inheritdoc/>
    public ModType Type { get; }

    /// <summary>
    /// Always return <see langword="null"/>, because mod instances cannot have a version range.
    /// </summary>
    public SemVersionRange? VersionRange => null;

    /// <inheritdoc cref="IModIdentity" />
    public override string Name { get; }

    /// <inheritdoc/>
    string IModIdentity.Name => Name;

    /// <inheritdoc/>
    public virtual IModinfo? ModInfo
    {
        get
        {
            if (_modinfoSearched)
                return _modInfo;
            if (_modInfo != null)
                return _modInfo;
            _modInfo = ResolveModInfo();
            _modinfoSearched = true;
            return _modInfo;
        }
    }

    /// <inheritdoc/>
    public SemVersion? Version => _modVersion ??= InitializeVersion();

    IModDependencyList IModIdentity.Dependencies =>
        new DependencyList(Dependencies.Select(d => d.Mod).OfType<IModReference>().ToList(), DependencyResolveLayout);

    /// <inheritdoc cref="IMod.Dependencies"/>
    public IReadOnlyList<ModDependencyEntry> Dependencies => DependenciesInternal.ToList();

    /// <inheritdoc/>
    public DependencyResolveStatus DependencyResolveStatus { get; protected set; }

    /// <inheritdoc/>
    public virtual DependencyResolveLayout DependencyResolveLayout =>
        ModInfo?.Dependencies.ResolveLayout ?? DependencyResolveLayout.ResolveRecursive;

    /// <summary>
    /// Creates a new <see cref="IMod"/> instances with a constant name
    /// </summary>
    /// <param name="game">The game of the mod</param>
    /// <param name="type">The mod's platform</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="name"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    protected ModBase(IGame game, ModType type, string name, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
        ServiceProvider = serviceProvider;
        Name = name;
        Game = game;
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="IMod"/> instances from a modinfo. The modinfo must not be <see langword="null"/>!
    /// </summary>
    /// <param name="game">The game of the mod</param>
    /// <param name="type">The mod's platform</param>
    /// <param name="modinfo">The modinfo data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="modinfo"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ModinfoException">when <paramref name="modinfo"/> is not valid.</exception>
    protected ModBase(IGame game, ModType type, IModinfo modinfo, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (modinfo == null)
            throw new ArgumentNullException(nameof(modinfo));
        modinfo.Validate();
        _modInfo = modinfo;
        ServiceProvider = serviceProvider;
        Game = game;
        Name = modinfo.Name;
        Type = type;
    }

    /// <inheritdoc/>
    public virtual void ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options)
    {
        if (resolver == null) 
            throw new ArgumentNullException(nameof(resolver));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (DependencyResolveStatus == DependencyResolveStatus.Resolving)
            throw new ModDependencyCycleException(this, "Already resolving the current instance's dependencies. Is there a Cycle?");

        try
        {
            DependencyResolveStatus = DependencyResolveStatus.Resolving;
            var oldList = DependenciesInternal.ToList();
            DependenciesInternal.Clear();
            var result = resolver.Resolve(this, options);
            DependenciesInternal.AddRange(result);
            OnDependenciesChanged(new ModDependenciesChangedEventArgs(this, oldList, result, DependencyResolveLayout));
            DependencyResolveStatus = DependencyResolveStatus.Resolved;
        }
        catch
        {
            DependencyResolveStatus = DependencyResolveStatus.Faulted;
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual bool Equals(IMod? other)
    {
        return ModEqualityComparer.Default.Equals(this, other);
    }

    /// <inheritdoc/>
    public virtual bool Equals(IModIdentity? other)
    {
        return ModEqualityComparer.Default.Equals(this, other);
    }

    /// <inheritdoc/>
    public virtual bool Equals(IModReference? other)
    {
        return ModEqualityComparer.Default.Equals(this, other);
    }
    
    /// <summary>
    /// Implementation to resolve the <see cref="ModInfo"/> property.
    /// </summary>
    /// <remarks>This implementation returns <see langword="null"/></remarks>
    /// <returns>The resolved <see cref="IModinfo"/>.</returns>
    protected virtual IModinfo? ResolveModInfoCore()
    {
        // Intentionally return null in base implementation,
        // since we cannot know which modinfo, if multiple found shall get used.
        return null;
    }

    /// <summary>
    /// Resolves the version of this mod.
    /// </summary>
    /// <remarks>The implementation returns the value from the <see cref="ModInfo"/>, or <see langword="null"/>.</remarks>
    /// <returns>The resolved version or <see langword="null"/>.</returns>
    protected virtual SemVersion? InitializeVersion()
    {
        return ModInfo?.Version;
    }

    /// <summary>
    /// Resolves the icon of this mod.
    /// </summary>
    /// <remarks
    /// >The implementation returns the value from <see cref="IModinfo.Icon"/> if present.
    /// If no modinfo is available or modinfo does not define an icon path, <see cref="IIconFinder"/> is used.
    /// </remarks>
    /// <returns>The resolved icon, or <see langword="null"/> if no icon is resolved.</returns>
    protected override string? ResolveIconFile()
    {
        var iconFile = ModInfo?.Icon;
        if (iconFile is not null)
            return iconFile;
        var finder = ServiceProvider.GetRequiredService<IIconFinder>();
        return finder.FindIcon(this);
    }

    /// <summary>
    /// Resolves the installed languages of this mod.
    /// </summary>
    /// <remarks>The implementation returns the value an <see cref="IModLanguageFinderFactory"/> service.</remarks>
    /// <returns>The resolved languages.</returns>
    protected override IReadOnlyCollection<ILanguageInfo> ResolveInstalledLanguages()
    {
        var factory = ServiceProvider.GetRequiredService<IModLanguageFinderFactory>();
        var finder = factory.CreateLanguageFinder(this);
        return finder.FindInstalledLanguages(this);
    }

    private IModinfo? ResolveModInfo()
    {
        var resolvingArgs = new ResolvingModinfoEventArgs(this);
        OnResolvingModinfo(resolvingArgs);
        IModinfo? modinfo = null;
        if (!resolvingArgs.Cancel)
            modinfo = ResolveModInfoCore();
        if (modinfo is null)
            return modinfo;
        OnModinfoResolved(new ModinfoResolvedEventArgs(this, modinfo));
        if (!new ModIdentityEqualityComparer(false, false, StringComparer.Ordinal).Equals(this, modinfo))
            throw new ModinfoException("Resolved modinfo does not match the current mod");
        return modinfo;
    }

    /// <summary>
    /// Raised the <see cref="ResolvingModinfo"/> event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected virtual void OnResolvingModinfo(ResolvingModinfoEventArgs e)
    {
        ResolvingModinfo?.Invoke(this, e);
    }

    /// <summary>
    /// Raised the <see cref="ModinfoResolved"/> event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected virtual void OnModinfoResolved(ModinfoResolvedEventArgs e)
    {
        ModinfoResolved?.Invoke(this, e);
    }

    /// <summary>
    /// Raised the <see cref="DependenciesChanged"/> event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected virtual void OnDependenciesChanged(ModDependenciesChangedEventArgs e)
    {
        DependenciesChanged?.Invoke(this, e);
    }

    string IConvertibleToJson.ToJson()
    {
        return new ModReference(this).ToJson();
    }

    void IConvertibleToJson.ToJson(Stream stream)
    {
        new ModReference(this).ToJson(stream);
    }
}