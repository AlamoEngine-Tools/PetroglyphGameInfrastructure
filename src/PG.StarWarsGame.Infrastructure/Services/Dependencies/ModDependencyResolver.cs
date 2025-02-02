using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal class ModDependencyResolver(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public IReadOnlyList<IMod> Resolve(IMod mod)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));

        if (mod.DependencyResolveStatus == DependencyResolveStatus.Resolved)
            return mod.Dependencies;

        var game = mod.Game;

        var graphBuilder = _serviceProvider.GetRequiredService<ModDependencyGraphBuilder>();
        var dependencyGraph = graphBuilder.Build(mod);

        if (dependencyGraph.HasCycle())
            throw new ModDependencyCycleException(mod, $"The mod '{mod}' has a dependency cycle.");

        ModDependencyGraphVertex rootVertex = null!;
        var directDeps = new List<IMod>();

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
                    var dep = game.FindMod(outEdge.Target.Mod);
                    directDeps.Add(dep!);
                }
            }
            else
            {
                if (!outEdges.Any())
                    continue;
                var currentMod = game.FindMod(vertex.Mod);
                Debug.Assert(currentMod is not null);
                currentMod!.ResolveDependencies();
            }
        }

        Debug.Assert(rootVertex is not null);
        return directDeps;
    }
}