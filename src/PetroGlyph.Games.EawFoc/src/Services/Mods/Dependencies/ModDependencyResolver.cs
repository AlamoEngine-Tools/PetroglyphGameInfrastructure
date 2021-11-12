using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies;

/// <summary>
/// Resolves mod dependencies specified in <see cref="IModIdentity.Dependencies"/> and returns them as an <see cref="T:IList&lt;IMod&gt;"/>.
/// </summary>
/// <remarks>Note, that this instance is not Thread-Safe!</remarks>
public class ModDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<IMod> _visitedMods = new(ModEqualityComparer.ExcludeDependencies);

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModDependencyResolver(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IList<ModDependencyEntry> Resolve(IMod mod, DependencyResolverOptions options)
    {
        Requires.NotNull(mod, nameof(mod));
        Requires.NotNull(options, nameof(options));

        //_visitedMods.Add(mod);

        var graphBuilder = _serviceProvider.GetService<IModDependencyGraphBuilder>() ?? new ModDependencyGraphBuilder();
        var dependencyGraph = graphBuilder.BuildResolveFree(mod);

        if (options.ResolveCompleteChain)
            ResolveDependencies(dependencyGraph, mod);

        if (options.CheckForCycle && dependencyGraph.HasCycle())
            throw new ModDependencyCycleException(mod, $"The mod {mod} has a dependency cycle");

        // We cannot use the graph here because DependenciesOf always returns the direct neighbors, 
        // meaning e.g. a FullResolved layout might not get returned in a whole by this.
        return graphBuilder.GetModDependencyListResolveFree(mod);
    }

    private void ResolveDependencies(IModDependencyGraph dependencyGraph, IMod root)
    {
        var queue = new Queue<IMod>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var source = queue.Dequeue();
            var layout = source.DependencyResolveLayout;

            var edges = dependencyGraph.DependenciesOf(source);

            if (!_visitedMods.Add(source))
                continue;

            for (var i = 0; i < edges.Count; i++)
            {
                if (layout == DependencyResolveLayout.ResolveRecursive ||
                    i == edges.Count - 1 && layout == DependencyResolveLayout.ResolveLastItem)
                {
                    queue.Enqueue(edges[i].Mod);
                }
            }

            if (source != root)
            {
                // We should not pass options with ResolveCompleteChain set, which would be truly recursive. 
                // This however would mean that in the event of a cycle
                // the whole chain would be in a undefined state,
                // where we could never trust the IMod.Dependencies property.
                // This way we make sure the property always yields the correct 1st-level dependencies.
                source.ResolveDependencies(this, new DependencyResolverOptions());
            }
        }
    }
}