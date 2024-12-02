using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Mods;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal class ModReferenceDependencyGraph : AdjacencyGraph<GraphModReference, ModReferenceEdge>
{
    public bool HasCycle()
    {
        return !this.IsDirectedAcyclicGraph();
    }

    internal IEnumerable<ModReferenceEdge> DependenciesOf(IMod mod)
    {
        var vertex = Vertices.FirstOrDefault(v => v.Mod.Equals(mod));
        Debug.Assert(vertex is not null, $"Unable to find mod '{mod}' in graph.");
        return OutEdges(vertex!);
    }
}