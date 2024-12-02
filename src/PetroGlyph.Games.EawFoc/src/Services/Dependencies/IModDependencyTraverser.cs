using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// Service which flattens the dependencies of mods as specified in
/// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#iv-mod-dependency-handling"/>
/// </summary>
public interface IModDependencyTraverser
{
    /// <summary>
    /// Flattens the dependency chain of the specified mod.
    /// </summary>
    /// <remarks>
    /// <paramref name="targetMod"/>is included to the result and is always the first item in the list.
    /// If <paramref name="targetMod"/> has no dependencies, it will be the only item in the list.
    /// </remarks>
    /// <param name="targetMod">The mod which dependencies shall get flattened.</param>
    /// <returns>A flattened list of the mod's dependencies. The <paramref name="targetMod"/> is the first item of the result.</returns>
    /// <exception cref="InvalidOperationException">The dependencies are not successfully resolved.</exception>
    /// <exception cref="ModDependencyException">The mod's dependencies could not be traversed</exception>
    IList<IMod> Traverse(IMod targetMod);
}