using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// An in-memory mod
/// </summary>
public sealed class VirtualMod : ModBase, IVirtualMod
{
    /// <summary>
    /// The identifier is a unique representation of the mod's name and its dependencies.
    /// </summary>
    public override string Identifier { get; }

    /// <inheritdoc/>
    public override DependencyResolveLayout DependencyResolveLayout { get; }


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="modInfoData">The mod's <see cref="IModinfo"/> data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ModException">If the <paramref name="modInfoData"/> has no dependencies.</exception>
    public VirtualMod(IGame game, IModinfo modInfoData, IServiceProvider serviceProvider)
        : base(game, ModType.Virtual, modInfoData, serviceProvider)
    {
        if (modInfoData.Dependencies is null || !modInfoData.Dependencies.Any())
            throw new PetroglyphException("Virtual mods must be initialized with pre-defined dependencies");

        // Since we are in a ctor we cannot use the common services.
        // We have to use a lightweight implementation for resolving and checking dependencies.
        // This means we cannot ensure at this point this instance has
        // a) A cycle free dependency graph,
        // b) at least one physical mod as a base.
        foreach (var dependency in modInfoData.Dependencies)
        {
            var mod = game.FindMod(dependency);
            DependenciesInternal.Add(new ModDependencyEntry(mod, dependency.VersionRange));
        }
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
    public VirtualMod(string name, IGame game, IList<ModDependencyEntry> dependencies, DependencyResolveLayout layout, IServiceProvider serviceProvider)
        : base(game, ModType.Virtual, name, serviceProvider)
    {
        Requires.NotNull(dependencies, nameof(dependencies));
        if (!dependencies.Any())
            throw new PetroglyphException("Virtual mods must be initialized with pre-defined dependencies");

        // Since we are in a ctor we cannot use the common services.
        // We have to use a lightweight implementation for resolving and checking dependencies.
        // This means we cannot ensure at this point this instance has
        // a) A cycle free dependency graph,
        // b) at least one physical mod as a base.
        foreach (var dependency in dependencies)
        {
            if (!dependency.Mod.Game.Equals(game))
                throw new PetroglyphException($"Game of mod {dependency} does not match this mod's game.");
            DependenciesInternal.Add(dependency);
        }
        DependencyResolveLayout = layout;
        DependencyResolveStatus = DependencyResolveStatus.Resolved;

        Identifier = CalculateIdentifier();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name + "-" + Identifier;
    }

    /// <summary>
    /// This method is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public override void ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options)
    {
        throw new NotSupportedException("Virtual mods cannot resolve their dependencies after initialization");
    }

    /// <summary>
    /// This method is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected override void OnResolvingModinfo(ResolvingModinfoEventArgs e)
    {
        throw new NotSupportedException("Virtual mods cannot lazy load modinfo data");
    }

    /// <summary>
    /// This method is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    protected override void OnDependenciesChanged(ModDependenciesChangedEventArgs e)
    {
        throw new NotSupportedException("Virtual mods cannot lazy load modinfo data");
    }

    private string CalculateIdentifier()
    {
        var builder = ServiceProvider.GetService<IModIdentifierBuilder>() ?? new ModIdentifierBuilder(ServiceProvider);
        return builder.Build(this);
    }
}