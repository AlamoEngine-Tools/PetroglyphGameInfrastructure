using System;
using System.Collections.Generic;
using System.Linq;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal class ModDependencyGraphBuilder
{
    public ModDependencyGraph Build(IMod rootMod)
    {
        if (rootMod == null)
            throw new ArgumentNullException(nameof(rootMod));
        var game = rootMod.Game;

        // Assure rootMod itself is added to the game.
        GetModOrThrow(game, rootMod);

        var graph = new ModDependencyGraph();
        graph.AddVertex(new ModDependencyGraphVertex(rootMod, DependencyKind.Root));

        var pendingQueue = new Queue<IMod>();
        pendingQueue.Enqueue(rootMod);

        var visitedMods = new HashSet<IMod>();

        while (pendingQueue.Count > 0)
        {
            var currentMod = pendingQueue.Dequeue();
            if (!visitedMods.Add(currentMod))
                continue;

            var dependencyList = ((IModIdentity)currentMod).Dependencies;
            if (dependencyList.Count == 0)
                continue;

            for (var i = 0; i < dependencyList.Count; i++)
            {
                var dependencyRef = dependencyList[i];
                var dependency = GetModOrThrow(game, dependencyRef);

                IsVersionMatchOrThrow(dependencyRef, dependency);

                var currentModVertex = graph.Vertices.FirstOrDefault(x => x.Mod.Equals(currentMod))
                                       ?? new ModDependencyGraphVertex(currentMod, DependencyKind.Transitive);

                var depKind = GetDependencyKind(rootMod, currentMod);

                graph.AddVerticesAndEdge(new ModDependencyGraphEdge(
                    currentModVertex,
                    new ModDependencyGraphVertex(dependency, depKind)));

                if (ShallEnqueueMod(dependencyList.ResolveLayout, i, dependencyList.Count))
                    pendingQueue.Enqueue(dependency);
            }
        }

        return graph;
    }

    private static void IsVersionMatchOrThrow(IModReference dependencyRef, IMod dependency)
    {
        if (dependency.Version is null || dependencyRef.VersionRange is null)
            return;

        if (!dependencyRef.VersionRange.Contains(dependency.Version))
            throw new VersionMismatchException(
                dependencyRef,
                dependency,
                $"Dependency '{dependency.Identifier}' with version '{dependency.Version}' does not match the expected version-range '{dependencyRef.VersionRange}'");
    }

    private static DependencyKind GetDependencyKind(IMod root, IMod currentMod)
    {
        return root.Equals(currentMod) ? DependencyKind.DirectDependency : DependencyKind.Transitive;
    }

    private static IMod GetModOrThrow(IGame game, IModReference modRef)
    {
        var mod = game.FindMod(modRef);
        if (mod is null)
            throw new ModNotFoundException(modRef, game);
        return mod;
    }

    private static bool ShallEnqueueMod(DependencyResolveLayout resolveLayout, int index, int maxCount)
    {
        switch (resolveLayout)
        {
            case DependencyResolveLayout.FullResolved:
                return false;
            case DependencyResolveLayout.ResolveRecursive:
                return true;
            case DependencyResolveLayout.ResolveLastItem:
            {
                return index == maxCount - 1;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(resolveLayout), resolveLayout, null);
        }
    }
}