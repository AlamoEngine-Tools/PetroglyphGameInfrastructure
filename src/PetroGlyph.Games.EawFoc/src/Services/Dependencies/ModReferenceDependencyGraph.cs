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