﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AET.Modinfo.Spec;
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

    /// <inheritdoc />
    /// <exception cref="ModDependencyException">No physical mod was found.</exception>
    protected override void OnDependenciesResolved()
    {
        if (Dependencies.Any(x => x is not IPhysicalMod))
            throw new ModDependencyException(this, null, "Virtual Mods must have at least one physical mod as dependency.");
        base.OnDependenciesResolved();
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Virtual Mod: {Name}; Identifier={Identifier}";
    }
}