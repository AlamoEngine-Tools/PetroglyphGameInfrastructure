using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Factory service to create a <see cref="ModArgumentList"/> from a given mod instance.
/// </summary>
public interface IModArgumentListFactory
{
    /// <summary>
    /// Builds a <see cref="ModArgumentList"/> for a given mod.
    /// <para>
    /// If a mod's dependencies have been resolved, the correct argument chain will be build;
    /// Otherwise only this instance will be added.
    /// </para>
    /// <para>
    /// Virtual mods will NOT be included to the argument list.
    /// </para>
    /// </summary>
    /// <param name="modInstance">The target mod which is requested for launching.</param>
    /// <param name="validateArgumentOnCreation">Validates the created mod argument and throws a <see cref="ModException"/> if it's not valid.</param>
    /// <returns>List of <see cref="ModArgument"/>.</returns>
    /// <exception cref="ModException">IF <paramref name="validateArgumentOnCreation"/> is <see langword="true"/>:
    /// The created argument will not be valid for execution </exception>
    /// <exception cref="ModDependencyCycleException">A mod dependency cycle was detected.</exception>
    ModArgumentList BuildArgumentList(IMod modInstance, bool validateArgumentOnCreation);

    /// <summary>
    /// Takes a fully traversed mod chain and builds an <see cref="ModArgumentList"/> from it.
    /// </summary>
    /// <para>
    /// Virtual mods will NOT be included to the argument list.
    /// </para>
    /// <param name="traversedModChain">The traversed mod chain.</param>
    /// <param name="validateArgumentOnCreation">Validates the created mod argument and throws a <see cref="ModException"/> if it's not valid.</param>
    /// <returns>List of <see cref="ModArgument"/>.</returns>
    /// /// <exception cref="ModException">IF <paramref name="validateArgumentOnCreation"/> is <see langword="true"/>:
    /// The created argument will not be valid for execution </exception>
    ModArgumentList BuildArgumentList(IList<IMod> traversedModChain, bool validateArgumentOnCreation);
}