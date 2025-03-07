﻿using System;
using System.Collections.Generic;
using System.IO;
using AET.Modinfo;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Equality;
using AET.Modinfo.Utilities;
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
    public event EventHandler? DependenciesResolved;

    private SemVersion? _modVersion;

    /// <inheritdoc/>
    public string Identifier { get; }

    /// <inheritdoc/>
    public override IGame Game { get; }

    /// <inheritdoc/>
    public ModType Type { get; }

    /// <remarks>
    /// Always returns <see langword="null"/>, because mod instances cannot have a version range.
    /// </remarks>
    /// <inheritdoc />
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
    /// Initializes a new instance of the <see cref="ModBase"/> class of the specified game, mod identifier, mod type, name and service provider.
    /// </summary>
    /// <param name="game">The game of the mod</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="type">The mod's platform</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="identifier"/> or <paramref name="name"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> or <paramref name="name"/> is empty.</exception>
    protected ModBase(IGame game, string identifier, ModType type, string name, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(identifier);
        ServiceProvider = serviceProvider;
        Identifier = identifier;
        Name = name;
        Game = game;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModBase"/> class of the specified game, mod identifier, mod type, modinfo data and service provider.
    /// </summary>
    /// <param name="game">The game of the mod</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="type">The mod's platform</param>
    /// <param name="modinfo">The modinfo data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="identifier"/> or <paramref name="modinfo"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> is empty.</exception>
    /// <exception cref="ModinfoException">when <paramref name="modinfo"/> is not valid.</exception>
    protected ModBase(IGame game, string identifier, ModType type, IModinfo modinfo, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        if (modinfo == null)
            throw new ArgumentNullException(nameof(modinfo));
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(identifier);
        modinfo.Validate();
        Identifier = identifier;
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
            var resolver = ServiceProvider.GetRequiredService<ModDependencyResolver>();
            Dependencies =  resolver.Resolve(this);
            OnDependenciesResolved();
            DependencyResolveStatus = DependencyResolveStatus.Resolved;
        }
        catch
        {
            DependencyResolveStatus = DependencyResolveStatus.Faulted;
            throw;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (GetType() != obj.GetType())
            return false;
        var other = (IMod)obj;
        return Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return ModEqualityComparer.Default.GetHashCode(this);
    }

    /// <inheritdoc/>
    public bool Equals(IMod? other)
    {
        return ModEqualityComparer.Default.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(IModIdentity? other)
    {
        return ModIdentityEqualityComparer.Default.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(IModReference? other)
    {
        return ModReferenceEqualityComparer.Default.Equals(this, other);
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
    protected virtual void OnDependenciesResolved()
    {
        DependenciesResolved?.Invoke(this, EventArgs.Empty);
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