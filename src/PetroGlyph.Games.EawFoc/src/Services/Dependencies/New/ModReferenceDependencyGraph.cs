using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies.New;

internal class ModReferenceDependencyGraph : AdjacencyGraph<GraphModReference, ModReferenceEdge>
{
    public bool HasCycle()
    {
        return !this.IsDirectedAcyclicGraph();
    }
}

internal sealed class GraphModReference(IModReference modReference, DependencyKind kind) : IEquatable<GraphModReference>
{
    public IModReference ModReference { get; } = modReference;

    public DependencyKind Kind { get; } = kind;

    public bool Equals(GraphModReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ModReference.Equals(other.ModReference);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is GraphModReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ModReference.GetHashCode();
    }
}

internal enum DependencyKind
{
    Root,
    DirectDependency,
    Transitive
}

internal sealed class ModReferenceEdge(GraphModReference source, GraphModReference target) : Edge<GraphModReference>(source, target), IEquatable<ModReferenceEdge>
{ 
    public bool Equals(ModReferenceEdge? other)
    {
        if (other is null)
            return false;
        return Source.Equals(other.Source) && Target.Equals(other.Target);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) 
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj is ModReferenceEdge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Target);
    }
}

internal class ModReferenceDependencyGraphBuilder
{
    private readonly IModIdentifierBuilder _identifierBuilder;

    public ModReferenceDependencyGraphBuilder(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        _identifierBuilder = serviceProvider.GetRequiredService<IModIdentifierBuilder>();
    }

    public ModReferenceDependencyGraph Build(IMod rootMod)
    {
        if (rootMod == null) 
            throw new ArgumentNullException(nameof(rootMod));
        var game = rootMod.Game;

        // Assure rootMod itself is added to the game.
        GetModOrThrow(game, rootMod);

        
        var graph = new ModReferenceDependencyGraph();
        graph.AddVertex(new GraphModReference(rootMod, DependencyKind.Root));

        var pendingQueue = new Queue<IMod>();
        pendingQueue.Enqueue(rootMod);

        var visitedMods = new HashSet<IModReference>();

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

                var currentModVertex = graph.Vertices.FirstOrDefault(x => x.ModReference.Equals(currentMod))
                                       ?? new GraphModReference(currentMod, DependencyKind.Transitive);

                var depKind = GetDependencyKind(rootMod, currentMod);

                graph.AddVerticesAndEdge(new ModReferenceEdge(
                    currentModVertex, 
                    new GraphModReference(dependency, depKind)));

                if (ShallEnqueueMod(dependencyList.ResolveLayout, i, dependencyList.Count)) 
                    pendingQueue.Enqueue(dependency);
            }
        }

        return graph;
    }

    private static DependencyKind GetDependencyKind(IMod root, IMod currentMod)
    {
        if (root.Equals(currentMod))
            return DependencyKind.DirectDependency;
        return DependencyKind.Transitive;
    }

    private IMod GetModOrThrow(IGame game, IModReference modRef)
    {
        var mod = game.FindMod(_identifierBuilder.Normalize(modRef));

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
                if (index == maxCount - 1)
                    return true;
                return false;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(resolveLayout), resolveLayout, null);
        }
    }
}

internal class NewModDependencyResolver(IServiceProvider serviceProvider)
{
    public IReadOnlyList<ModDependencyEntry> Resolve(IMod mod)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));

        if (mod.DependencyResolveStatus == DependencyResolveStatus.Resolved)
            return mod.Dependencies;

        var game = mod.Game;

        var graphBuilder = serviceProvider.GetRequiredService<ModReferenceDependencyGraphBuilder>();
        var dependencyGraph = graphBuilder.Build(mod);

        if (dependencyGraph.HasCycle())
            throw new ModDependencyCycleException(mod, $"The mod '{mod}' has a dependency cycle.");

        GraphModReference rootVertex = null!;
        var directDeps = new List<ModDependencyEntry>();

        // Resolve all dependencies as specified by the resolve layout. 
        // This way we support strange things like:
        //              A : B, C, D [FullResolved]
        //      where   D : B
        // If we would resolve this fully recursive we would have a cycle.
        // However, as the dependency list of 'A' is FullResolved, the cycle never occurs at play time of 'A'.
        // Thus, we do not resolve the dependencies of 'D'.
        // This means we only need to resolve dependencies which have out-going edges in the graph.
        // In other words, if vertex is a source of at least one edge, we resolve it.
        foreach (var vertex in dependencyGraph.Vertices)
        {
            var outEdges = dependencyGraph.OutEdges(vertex);

            if (vertex.Kind == DependencyKind.Root)
            {
                Debug.Assert(rootVertex is null);
                rootVertex = vertex;
                
                foreach (var outEdge in outEdges)
                {
                    var dep = game.FindMod(outEdge.Target.ModReference);
                    directDeps.Add(new ModDependencyEntry(dep!, null));
                }
            }
            else
            {
                if (!outEdges.Any())
                    continue;
                var currentMod = game.FindMod(vertex.ModReference);
                Debug.Assert(currentMod is not null);
                currentMod!.ResolveDependencies();
            }
        }

        Debug.Assert(rootVertex is not null);
        return directDeps;
    }
}