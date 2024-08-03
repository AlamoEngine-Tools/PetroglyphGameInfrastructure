using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// Data Structure which represents a mod's dependency graph.
/// Enumerating this instance yields all Mods in this graph in arbitrary order.
/// </summary>
internal interface IModDependencyGraph : IEnumerable<ModDependencyEntry>
{
    /// <summary>
    /// Checks if this graph has a cycle.
    /// </summary>
    /// <returns><see langword="true"/> if a cycle is present; <see langword="false"/> otherwise.</returns>
    bool HasCycle();

    /// <summary>
    /// Returns all direct neighbors of the <paramref name="sourceEntry"/> in correct left-right order.
    /// </summary>
    /// <remarks><see cref="ModDependencyEntry.VersionRange"/> gets ignored.</remarks>
    /// <param name="sourceEntry">The edges' source entry.</param>
    IList<ModDependencyEntry> DependenciesOf(ModDependencyEntry sourceEntry);

    /// <summary>
    /// Returns all direct neighbors of the <paramref name="sourceMod"/> in correct left-right order
    /// </summary>
    /// <param name="sourceMod">The edges' source mod.</param>
    IList<ModDependencyEntry> DependenciesOf(IMod sourceMod);
}