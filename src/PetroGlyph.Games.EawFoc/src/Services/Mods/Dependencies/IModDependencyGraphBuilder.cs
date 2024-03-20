using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// Service to build an <see cref="IModDependencyGraph"/> for a given <see cref="IMod"/>
/// </summary>
public interface IModDependencyGraphBuilder
{
    /// <summary>
    /// Builds an <see cref="IModDependencyGraph"/> from a mod's resolved <see cref="IMod.Dependencies"/> list.
    /// </summary>
    /// <param name="rootMod">the target mod to build the graph for.</param>
    /// <returns>The built dependency graph.</returns>
    /// <exception cref="ModException">if a mod's dependencies have not yet been resolved.</exception>
    IModDependencyGraph Build(IMod rootMod);

    /// <summary>
    /// Builds an <see cref="IModDependencyGraph"/>.
    /// Tries to take dependencies from an already resolved <see cref="IMod.Dependencies"/>.
    /// If the mod still is unresolved, dependencies get taken directly from the <see cref="IMod.ModInfo"/> data, if present.
    /// </summary>
    /// <param name="rootMod">the target mod to build the graph for.</param>
    /// <returns>The built dependency graph.</returns>
    /// <exception cref="ModNotFoundException">if a dependency (<see cref="IModReference"/>) could not be found in the <paramref name="rootMod"/>'s game..</exception>
    IModDependencyGraph BuildResolveFree(IMod rootMod);

    /// <summary>
    /// Tries to build an <see cref="IModDependencyGraph"/> from a mod's resolved <see cref="IMod.Dependencies"/> list.
    /// </summary>
    /// <param name="rootMod">the target mod to build the graph for.</param>
    /// <param name="graph">The built dependency graph.</param>
    /// <returns><see langword="true"/>if a graph was built successfully; <see langword="false"/> otherwise.</returns>
    bool TryBuild(IMod rootMod, out IModDependencyGraph? graph);

    /// <summary>
    /// Tries to build an <see cref="IModDependencyGraph"/>.
    /// Tries to take dependencies from an already resolved <see cref="IMod.Dependencies"/>.
    /// If the mod still is unresolved, dependencies get taken directly from the <see cref="IMod.ModInfo"/> data, if present.
    /// </summary>
    /// <param name="rootMod">the target mod to build the graph for.</param>
    /// <param name="graph">The built dependency graph.</param>
    /// <returns><see langword="true"/>if a graph was built successfully; <see langword="false"/> otherwise.</returns>
    bool TryBuildResolveFree(IMod rootMod, out IModDependencyGraph? graph);

    /// <summary>
    /// Gets a mod's dependencies either from the <see cref="IMod.Dependencies"/> list, if the mod is already resolved
    /// or otherwise it searches for mods from the <see cref="IMod.ModInfo"/> data in the <paramref name="targetMod"/> game instance.
    /// If no modinfo is present an empty list will be returned.
    /// </summary>
    /// <remarks>The <paramref name="targetMod"/> is not part of the returned list.</remarks>
    /// <param name="targetMod">The mod to get the dependencies for.</param>
    /// <returns>The list of dependencies.</returns>
    IList<ModDependencyEntry> GetModDependencyListResolveFree(IMod targetMod);
}