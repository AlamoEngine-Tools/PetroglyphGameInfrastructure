using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Represents a virtual mod which only exists in memory.
/// </summary>
public sealed class VirtualMod : ModBase, IVirtualMod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMod"/> class of the specified game and modinfo.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="modInfoData">The mod's <see cref="IModinfo"/> data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="modInfoData"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ModException">If the <paramref name="modInfoData"/> has no dependencies.</exception>
    public VirtualMod(IGame game, string identifier, IModinfo modInfoData, IServiceProvider serviceProvider)
        : base(game, identifier, ModType.Virtual, modInfoData, serviceProvider)
    {
        if (modInfoData.Dependencies.Count == 0)
            throw new ModException(this, "Virtual mods must be initialized with pre-defined dependencies");

        ModInfo = modInfoData;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMod"/> class of the specified game and name and dependency list.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="dependencies">list of dependencies.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ModException">If the <paramref name="dependencies"/> is empty.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or <paramref name="game"/> or <paramref name="dependencies"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public VirtualMod(IGame game, string identifier, string name, IModDependencyList dependencies, IServiceProvider serviceProvider)
        : base(game, identifier, ModType.Virtual, name, serviceProvider)
    {
        if (dependencies == null) 
            throw new ArgumentNullException(nameof(dependencies));
        if (dependencies.Count == 0)
            throw new ModException(this, "Virtual mods must have at least one physical dependency.");

        ModInfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(dependencies)
        };
    }

    /// <inheritdoc />
    protected override IReadOnlyList<IMod> ResolveDependenciesCore()
    {
        var dependencies = base.ResolveDependenciesCore();
        if (dependencies.Any(x => x is not IPhysicalMod))
            throw new ModException(this, "Virtual Mods must have at least one physical mod as dependency.");
        return dependencies;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Identifier;
    }
}