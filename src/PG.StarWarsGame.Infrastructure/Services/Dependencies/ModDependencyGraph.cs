using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Mods;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal class ModDependencyGraph : AdjacencyGraph<ModDependencyGraphVertex, ModDependencyGraphEdge>
{
    public bool HasCycle()
    {
        return !this.IsDirectedAcyclicGraph();
    }

    internal IEnumerable<ModDependencyGraphEdge> DependenciesOf(IMod mod)
    {
        var vertex = Vertices.FirstOrDefault(v => v.Mod.Equals(mod));
        Debug.Assert(vertex is not null, $"Unable to find mod '{mod}' in graph.");
        return OutEdges(vertex!);
    }
}