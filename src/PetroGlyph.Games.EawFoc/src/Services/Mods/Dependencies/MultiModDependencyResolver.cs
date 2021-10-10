using System;
using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    /// <summary>
    /// Service which resolves dependencies of many mods coordinated by minimizing workload.
    /// </summary>
    public class MultiModDependencyResolver
    {
        /// <summary>
        /// Pass-Through event from <see cref="IMod.DependenciesChanged"/> for every processed mod by this instance.
        /// </summary>
        public event EventHandler<ModDependenciesChangedEventArgs>? DependenciesChanged;

        private readonly IDependencyResolver _resolver;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="resolver">The resolver that shall get used.</param>
        public MultiModDependencyResolver(IDependencyResolver resolver)
        {
            Requires.NotNull(resolver, nameof(resolver));
            _resolver = resolver;
        }

        /// <summary>
        /// Resolves the dependencies of a given enumeration of mods.
        /// </summary>
        /// <remarks>This call always sets the <see cref="IMod.Dependencies"/> list.</remarks>
        /// <param name="modsToResolve">The mods to resolve.</param>
        /// <param name="options">The resolve options for the internal resolver.</param>
        /// <param name="skipResolvedMods">When set to <see langword="true"/> the methods does not resolve a mod which has its <see cref="IMod.DependencyResolveStatus"/> set to <see cref="DependencyResolveStatus.Resolved"/>.</param>
        /// <param name="abortOnError">When set to <see langword="true"/> the operation aborts immediately if an exception was thrown.
        /// Otherwise the operation will continue resolving mod dependencies.</param>
        /// <returns>Information if and errors happened during the operation.</returns>
        public MultiResolveResult ResolveDependenciesForMods(
            IEnumerable<IMod> modsToResolve, 
            DependencyResolverOptions options, 
            bool skipResolvedMods,
            bool abortOnError)
        {
            Requires.NotNull(modsToResolve, nameof(modsToResolve));
            Requires.NotNull(options, nameof(options));

            var result = new MultiResolveResult();

            foreach (var mod in modsToResolve)
            {
                if (skipResolvedMods && mod.DependencyResolveStatus == DependencyResolveStatus.Resolved)
                    continue;
                try
                {
                    mod.DependenciesChanged += OnDependenciesResolved;
                    mod.ResolveDependencies(_resolver, options);
                }
                catch (Exception e)
                {
                    result.AddError(mod, e);
                    if (abortOnError)
                        break;
                }
                finally
                {
                    mod.DependenciesChanged += OnDependenciesResolved;
                }
            }
            return result;
        }

        private void OnDependenciesResolved(object sender, ModDependenciesChangedEventArgs e)
        {
            // Just pass-through the event.
            DependenciesChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Data to represent errors during resolve operation.
        /// </summary>
        public class MultiResolveResult
        {
            private readonly Dictionary<IMod, Exception> _errorData = new();
            
            /// <summary>
            /// Indicates whether the operation had any errors.
            /// </summary>
            public bool HasErrors => _errorData.Any();

            /// <summary>
            /// Collection which holds the exception and the root mod which caused the error during a resolve process.
            /// </summary>
            public IReadOnlyCollection<(IMod, Exception)> ErrorData =>
                _errorData.Select(x => new ValueTuple<IMod, Exception>(x.Key, x.Value)).ToList();

            internal MultiResolveResult()
            {
            }
            
            internal void AddError(IMod mod, Exception error)
            {
                Requires.NotNull(mod, nameof(mod));
                Requires.NotNull(error, nameof(error));
                _errorData[mod] = error;
            }
        }
    }
}