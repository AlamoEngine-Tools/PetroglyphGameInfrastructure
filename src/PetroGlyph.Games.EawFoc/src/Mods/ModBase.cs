using System;
using System.Collections.Generic;
using System.IO;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using Semver;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// The base implementation for Mods.
/// </summary>
public abstract class ModBase : PlayableModContainer, IMod
{ 
    /// <inheritdoc/>
    public event EventHandler<ModDependenciesResolvedEventArgs>? DependenciesResolved;

    private SemVersion? _modVersion;

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
    public IModinfo? ModInfo { get; protected init; }

    /// <inheritdoc/>
    public SemVersion? Version => _modVersion ??= InitializeVersion();

    /// <summary>
    /// Gets the mod's dependency list from the modinfo data or an empty list if no modinfo data is specified.
    /// </summary>
    IModDependencyList IModIdentity.Dependencies => ModInfo?.Dependencies ?? DependencyList.EmptyDependencyList;

    /// <inheritdoc cref="IMod.Dependencies"/>
    public IReadOnlyList<IMod> Dependencies { get; protected set; } = [];

    /// <inheritdoc/>
    public DependencyResolveStatus DependencyResolveStatus { get; protected set; }

    /// <inheritdoc/>
    public DependencyResolveLayout DependencyResolveLayout =>
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
        ModInfo = modinfo;
        ServiceProvider = serviceProvider;
        Game = game;
        Name = modinfo.Name;
        Type = type;
    }

    /// <inheritdoc />
    public void ResolveDependencies()
    {
        if (DependencyResolveStatus == DependencyResolveStatus.Resolved)
            return;
        if (DependencyResolveStatus == DependencyResolveStatus.Resolving)
            throw new ModDependencyCycleException(this, "Already resolving the current instance's dependencies. Possible dependency cycle?");

        try
        {
            DependencyResolveStatus = DependencyResolveStatus.Resolving;
            var dependencies = ResolveDependenciesCore();
            Dependencies = dependencies ?? throw new PetroglyphException("Resolved dependency list is null!");
            OnDependenciesResolved(new ModDependenciesResolvedEventArgs(this));
            DependencyResolveStatus = DependencyResolveStatus.Resolved;
        }
        catch
        {
            DependencyResolveStatus = DependencyResolveStatus.Faulted;
            throw;
        }
    }

    /// <summary>
    /// Resolves the dependencies of the mod.
    /// </summary>
    /// <returns>The resolved dependencies as specified by the resolve layout.</returns>
    protected virtual IReadOnlyList<IMod> ResolveDependenciesCore()
    {
        var resolver = ServiceProvider.GetRequiredService<ModDependencyResolver>(); 
        return resolver.Resolve(this);
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
    /// Resolves the version of this mod.
    /// </summary>
    /// <remarks>The implementation returns the value from the <see cref="ModInfo"/>, or <see langword="null"/>.</remarks>
    /// <returns>The resolved version or <see langword="null"/>.</returns>
    protected virtual SemVersion? InitializeVersion()
    {
        return ModInfo?.Version;
    }

    /// <summary>
    /// Raises the <see cref="DependenciesResolved"/> event.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected virtual void OnDependenciesResolved(ModDependenciesResolvedEventArgs e)
    {
        DependenciesResolved?.Invoke(this, e);
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