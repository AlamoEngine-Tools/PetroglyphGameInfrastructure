using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <inheritdoc cref="IModDependencyTraverser"/>
internal class ModDependencyTraverser(IServiceProvider serviceProvider) : IModDependencyTraverser
{
    private readonly ModDependencyGraphBuilder _graphBuilder = serviceProvider.GetRequiredService<ModDependencyGraphBuilder>();

    public IList<IMod> Traverse(IMod targetMod)
    {
        if (targetMod.DependencyResolveStatus != DependencyResolveStatus.Resolved)
            throw new InvalidOperationException($"Dependencies of '{targetMod}' are not resolved.");

        var dependencyGraph = _graphBuilder.Build(targetMod);
        Debug.Assert(!dependencyGraph.HasCycle(), "resolved dependencies should never have a cycle!");

        var result = TraverseCore(dependencyGraph, targetMod, new List<IMod>(), new());
        RemoveDuplicates(result);
        return result.ToList();
    }

    private static void RemoveDuplicates(ICollection<IMod> list)
    {
        var known = new HashSet<IMod>();
        var entriesToRemove = new List<IMod>();

        foreach (var entry in list.Reverse())
        {
            if (!known.Add(entry)) 
                entriesToRemove.Add(entry);
        }

        foreach (var entry in entriesToRemove)
        {
            list.Remove(entry);
        }
    }

    private static IList<IMod> TraverseCore(
        ModDependencyGraph graph,
        IMod head,
        IList<IMod> result,
        Queue<IMod> queue)
    {
        result.Add(head);

        foreach (var edge in graph.DependenciesOf(head)) 
            queue.Enqueue(edge.Target.Mod);
        return !queue.Any() ? result : TraverseCore(graph, queue.Dequeue(), result, queue);
    }
}