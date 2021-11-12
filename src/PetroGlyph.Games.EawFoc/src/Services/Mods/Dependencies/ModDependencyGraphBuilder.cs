using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies;

/// <inheritdoc cref="IModDependencyGraphBuilder"/>
public class ModDependencyGraphBuilder : IModDependencyGraphBuilder
{
    private static readonly IList<ModDependencyEntry> EmptyDependencyList = new List<ModDependencyEntry>();


    /// <inheritdoc/>
    public IModDependencyGraph Build(IMod rootMod)
    {
        Requires.NotNull(rootMod, nameof(rootMod));
        var graph = new ModDependencyGraph();
        AddToGraph(graph, rootMod, ResolvingDependenciesFactory);
        return graph;
    }

    /// <inheritdoc/>
    public IModDependencyGraph BuildResolveFree(IMod rootMod)
    {
        Requires.NotNull(rootMod, nameof(rootMod));
        var graph = new ModDependencyGraph();
        AddToGraph(graph, rootMod, mod =>
        {
            var (modDependencyEntries, dependencyResolveLayout) = ResolveFreeDependenciesFactory(mod);
            return (modDependencyEntries.Select(i => i.Mod).ToList(), dependencyResolveLayout);
        });
        return graph;
    }

    /// <inheritdoc/>
    public bool TryBuild(IMod rootMod, out IModDependencyGraph? graph)
    {
        Requires.NotNull(rootMod, nameof(rootMod));
        graph = default;
        try
        {
            graph = Build(rootMod);
            return true;
        }
        catch (ModException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TryBuildResolveFree(IMod rootMod, out IModDependencyGraph? graph)
    {
        Requires.NotNull(rootMod, nameof(rootMod));
        graph = default;
        try
        {
            graph = BuildResolveFree(rootMod);
            return true;
        }
        catch (ModException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IList<ModDependencyEntry> GetModDependencyListResolveFree(IMod targetMod)
    {
        Requires.NotNull(targetMod, nameof(targetMod));
        return ResolveFreeDependenciesFactory(targetMod).Item1;
    }

    private void AddToGraph(ModDependencyGraph graph, IMod root, Func<IMod, (IList<IMod>, DependencyResolveLayout)> dependencyFactory)
    {
        var pendingQueue = new Queue<IMod>();
        pendingQueue.Enqueue(root);

        var visitedMods = new HashSet<IMod>();
        graph.AddVertex(new ModDependencyEntry(root));

        while (pendingQueue.Count > 0)
        {
            var source = pendingQueue.Dequeue();
            if (!visitedMods.Add(source))
                continue;

            var (dependencies, layout) = dependencyFactory(source);

            if (!dependencies.Any())
                continue;

            for (var i = 0; i < dependencies.Count; i++)
            {
                var dependency = dependencies[i];
                graph.AddDependency(source, dependency);

                if (layout == DependencyResolveLayout.ResolveRecursive ||
                    i == dependencies.Count - 1 && layout == DependencyResolveLayout.ResolveLastItem)
                {
                    pendingQueue.Enqueue(dependency);
                }
            }
        }
    }

    private static (IList<IMod>, DependencyResolveLayout) ResolvingDependenciesFactory(IMod source)
    {
        if (source.DependencyResolveStatus != DependencyResolveStatus.Resolved)
            throw new ModException(source, $"Mod {source} is not yet resolved.");
        return (source.Dependencies.Select(d => d.Mod).ToList(), source.DependencyResolveLayout);
    }

    private (IList<ModDependencyEntry>, DependencyResolveLayout) ResolveFreeDependenciesFactory(IMod source)
    {
        var layout = source.DependencyResolveLayout;
        if (source.DependencyResolveStatus == DependencyResolveStatus.Resolved)
            return (source.Dependencies.ToList(), source.DependencyResolveLayout);

        if (source.ModInfo is null)
            return (EmptyDependencyList, layout);

        layout = source.ModInfo.Dependencies.ResolveLayout;
        var dependencies = source.ModInfo.Dependencies
            .Select(modReference => new ModDependencyEntry(source.Game.FindMod(modReference), modReference.VersionRange))
            .ToList();
        return (dependencies, layout);
    }
}