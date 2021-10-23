using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies;

/// <summary>
/// Service which flattens the dependencies of a given <see cref="IMod"/> as specified in
/// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#iv-mod-dependency-handling"/>
/// </summary>
public interface IModDependencyTraverser
{
    /// <summary>
    /// Flattens the dependency chain of <paramref name="targetMod"/>.
    /// </summary>
    /// <remarks><paramref name="targetMod"/>will be included to the result and is always the first item in the list.
    /// If <paramref name="targetMod"/> has no dependencies, it will be the only item in the list.</remarks>
    /// <param name="targetMod">The mod which dependencies shall get flattened.</param>
    /// <returns>A flattened list of the mod's dependencies. The <paramref name="targetMod"/> is the first item of the result.</returns>
    /// <exception cref="ModDependencyCycleException">if the mod has a dependency cycle.</exception>
    IList<ModDependencyEntry> Traverse(IMod targetMod);
}