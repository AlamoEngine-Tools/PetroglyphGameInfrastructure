using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// Resolves mod dependencies specified in <see cref="IModIdentity.Dependencies"/> and returns them as an <see cref="T:IList&lt;IMod&gt;"/>.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// Resolves the dependencies of the specified mod.
    /// </summary>
    /// <remarks>
    /// This method does not set the <see cref="IMod.Dependencies"/> of <paramref name="mod"/>.
    /// If <paramref name="options"/> has <see cref="DependencyResolverOptions.CheckForCycle"/> is <see langword="true"/>,
    /// the resolved dependencies of <paramref name="mod"/> get their <see cref="IMod.Dependencies"/> updated.
    /// </remarks>
    /// <param name="mod">The mod to resolve dependencies for.</param>
    /// <param name="options">Options that specify how dependencies get resolved.</param>
    /// <returns>A sorted list of dependencies.</returns>
    /// <exception cref="ModDependencyCycleException"><paramref name="options"/> has <see cref="DependencyResolverOptions.CheckForCycle"/> set to <see langword="true"/> and a cycle is present.</exception>
    /// <exception cref="ModNotFoundException">If a dependency could not be found.</exception>
    IList<ModDependencyEntry> Resolve(IMod mod, DependencyResolverOptions options);
}