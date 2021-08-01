using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    /// <summary>
    /// Resolves mod dependencies specified in <see cref="IModIdentity.Dependencies"/> and returns them as an <see cref="T:IList&lt;IMod&gt;"/>.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Resolves the dependencies of a given mod.
        /// </summary>
        /// <remarks>
        /// This methods does not set the <see cref="IMod.Dependencies"/> of the <paramref name="mod"/> parameter.
        /// However, if <paramref name="options"/> has <see cref="DependencyResolverOptions.CheckForCycle"/> is <see langword="true"/>,
        /// the resolved dependencies get their <see cref="IMod.Dependencies"/> updated.
        /// </remarks>
        /// <param name="mod">The target mod</param>
        /// <param name="options">Options how the resolver behaves.</param>
        /// <returns>A sorted list of dependencies. Result depends of the <paramref name="options"/>.</returns>
        /// <exception cref="ModDependencyCycleException">If <paramref name="options"/> has <see cref="DependencyResolverOptions.CheckForCycle"/> is <see langword="true"/> and a cycle is present.</exception>
        /// <exception cref="ModNotFoundException">If a dependency could not be found.</exception>
        IList<ModDependencyEntry> Resolve(IMod mod, DependencyResolverOptions options);
    }
}