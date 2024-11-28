using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

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

                IsVersionMatchOrThrow(dependencyRef, dependency);

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

/// <summary>
/// 
/// </summary>
public class VersionMismatchException : ModDependencyException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    public VersionMismatchException(IModReference source, IModReference dependency) : base(source, dependency)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    public VersionMismatchException(IModReference source, IModReference dependency, string message) : base(source, dependency, message)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public VersionMismatchException(IModReference source, IModReference dependency, string message, Exception exception) : base(source, dependency, message, exception)
    {
    }
}

/// <summary>
/// 
/// </summary>
public class ModDependencyException : ModException
{
    /// <summary>
    /// 
    /// </summary>
    public IModReference Dependency { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    public ModDependencyException(IModReference source, IModReference dependency) : base(source)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    public ModDependencyException(IModReference source, IModReference dependency, string message) 
        : base(source, message)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dependency"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public ModDependencyException(IModReference source, IModReference dependency, string message, Exception exception) 
        : base(source, message, exception)
    {
        Dependency = dependency;
    }
}