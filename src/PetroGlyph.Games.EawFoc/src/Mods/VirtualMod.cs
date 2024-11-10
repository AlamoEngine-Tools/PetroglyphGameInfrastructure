using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Represents a virtual, in-memory mod.
/// </summary>
public sealed class VirtualMod : ModBase, IVirtualMod
{
    /// <inheritdoc />
    public override IModinfo? ModInfo { get; }

    /// <summary>
    /// The identifier is a unique representation of the mod's name and its dependencies.
    /// </summary>
    public override string Identifier { get; }

    /// <inheritdoc/>
    public override DependencyResolveLayout DependencyResolveLayout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMod"/> class of the specified game and modinfo.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="modInfoData">The mod's <see cref="IModinfo"/> data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="modInfoData"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PetroglyphException">If the <paramref name="modInfoData"/> has no dependencies.</exception>
    /// <exception cref="ModNotFoundException">A dependency was not found.</exception>
    /// <exception cref="ModException">No physical dependency was not found.</exception>
    public VirtualMod(IGame game, IModinfo modInfoData, IServiceProvider serviceProvider)
        : base(game, ModType.Virtual, modInfoData, serviceProvider)
    {
        if (modInfoData.Dependencies is null)
            throw new ArgumentException("dependencies cannot be null.", nameof(modInfoData));
        if (modInfoData.Dependencies.Count == 0)
            throw new PetroglyphException("Virtual mods must be initialized with pre-defined dependencies");

        ModInfo = modInfoData;

        // Since we are in a ctor we cannot use the common services.
        // We have to use a lightweight implementation for resolving and checking dependencies.
        // This means we cannot ensure at this point this instance has
        // a) A cycle free dependency graph,
        var hasPhysicalMod = false;
        foreach (var dependency in modInfoData.Dependencies)
        {
            var mod = game.FindMod(dependency);
            if (mod is null)
                throw new ModNotFoundException(dependency, this);
            DependenciesInternal.Add(new ModDependencyEntry(mod, dependency.VersionRange));
            if (mod.Type is ModType.Default or ModType.Workshops)
                hasPhysicalMod = true;
        }

        if (!hasPhysicalMod)
            throw new ModException(this, "No physical dependency was found.");

        DependencyResolveLayout = modInfoData.Dependencies.ResolveLayout;
        DependencyResolveStatus = DependencyResolveStatus.Resolved;

        Identifier = CalculateIdentifier();
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="name">The name of the mod.</param>
    /// <param name="game">The game of the mod.</param>
    /// <param name="layout">The layout of the <paramref name="dependencies"/> list.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="dependencies">list of dependencies.</param>
    /// <exception cref="ModException">If the <paramref name="dependencies"/> is empty.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or <paramref name="game"/> or <paramref name="dependencies"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public VirtualMod(string name, IGame game, IList<ModDependencyEntry> dependencies, DependencyResolveLayout layout, IServiceProvider serviceProvider)
        : base(game, ModType.Virtual, name, serviceProvider)
    {
        if (dependencies == null) 
            throw new ArgumentNullException(nameof(dependencies));
        if (dependencies.Count == 0)
            throw new PetroglyphException("Virtual mods must have at least one physical dependency.");

        // Since we are in a ctor we cannot use the common services.
        // We have to use a lightweight implementation for resolving and checking dependencies.
        // This means we cannot ensure at this point this instance has a cycle free dependency graph.
        foreach (var dependency in dependencies)
        {
            if (!dependency.Mod.Game.Equals(game))
                throw new PetroglyphException($"Game of mod {dependency} does not match this mod's game.");
            DependenciesInternal.Add(dependency);
        }
        DependencyResolveLayout = layout;
        DependencyResolveStatus = DependencyResolveStatus.Resolved;

        ModInfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(new List<IModReference>(Dependencies.Select(x => x.Mod)), layout)
        };

        Identifier = CalculateIdentifier();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name + "-" + Identifier;
    }

    /// <summary>
    /// This method is not supported, as virtual mods have pre-defined dependencies.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public override void ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options)
    {
        throw new NotSupportedException("Virtual mods cannot resolve their dependencies after initialization");
    }

    /// <summary>
    /// This method is not supported, as virtual mods have pre-defined dependencies.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected override void OnResolvingModinfo(ResolvingModinfoEventArgs e)
    {
        throw new NotSupportedException("Virtual mods cannot lazy load modinfo data");
    }

    /// <summary>
    /// This method is not supported, as virtual mods have pre-defined dependencies.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected override void OnDependenciesChanged(ModDependenciesChangedEventArgs e)
    {
        throw new NotSupportedException("Virtual mods cannot lazy load modinfo data");
    }

    private string CalculateIdentifier()
    {
        var builder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
        return builder.Build(this);
    }
}