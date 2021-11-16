using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Factory service to create a <see cref="ModArgumentList"/> from a given mod instance.
/// </summary>
public interface IModArgumentListFactory
{
    /// <summary>
    /// Builds a <see cref="ModArgumentList"/> for a given mod.
    /// <para>
    /// If a mod's dependencies have been resolved, the correct argument chain will be build. 
    /// </para>
    /// <para>
    /// Virtual mods will NOT be included to the argument list.
    /// </para>
    /// </summary>
    /// <param name="modInstance">The target mod which is requested for launching.</param>
    /// <returns>List of <see cref="ModArgument"/>.</returns>
    ModArgumentList BuildArgumentList(IMod modInstance);

    /// <summary>
    /// Takes a fully traversed mod chain and builds an <see cref="ModArgumentList"/> from it.
    /// </summary>
    /// <para>
    /// Virtual mods will NOT be included to the argument list.
    /// </para>
    /// <param name="traversedModChain">The traversed mod chain.</param>
    /// <returns>List of <see cref="ModArgument"/>.</returns>
    ModArgumentList BuildArgumentList(IList<IMod> traversedModChain);
}