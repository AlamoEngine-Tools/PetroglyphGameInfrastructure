using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    /// <inheritdoc cref="IModDependencyTraverser"/>
    public class ModDependencyTraverser : IModDependencyTraverser
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ModDependencyTraverser(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IList<ModDependencyEntry> Traverse(IMod targetMod)
        {
            var graphBuilder = _serviceProvider.GetService<IModDependencyGraphBuilder>() ??
                               new ModDependencyGraphBuilder();
            var dependencyGraph = graphBuilder.Build(targetMod);
            if (dependencyGraph.HasCycle())
                throw new ModDependencyCycleException($"Cycle detected while traversing {targetMod}.");

            var result = TraverseCore(dependencyGraph, new ModDependencyEntry(targetMod), new List<ModDependencyEntry>(), new Queue<ModDependencyEntry>());
            RemoveDuplicates(result);
            return result.ToList();
        }

        private static void RemoveDuplicates(ICollection<ModDependencyEntry> list)
        {
            var known = new HashSet<ModDependencyEntry>();
            var entriesToRemove = new List<ModDependencyEntry>();

            foreach (var entry in list.Reverse())
            {
                if (known.Contains(entry))
                {
                    entriesToRemove.Add(entry);
                    continue;
                }

                known.Add(entry);
            }

            foreach (var entry in entriesToRemove)
            {
                list.Remove(entry);
            }
        }

        private static IList<ModDependencyEntry> TraverseCore(IModDependencyGraph graph, ModDependencyEntry head, IList<ModDependencyEntry> result, Queue<ModDependencyEntry> queue)
        {
            result.Add(head);
            foreach (var dependency in graph.DependenciesOf(head.Mod))
                queue.Enqueue(dependency);
            return !queue.Any() ? result : TraverseCore(graph, queue.Dequeue(), result, queue);
        }
    }
}