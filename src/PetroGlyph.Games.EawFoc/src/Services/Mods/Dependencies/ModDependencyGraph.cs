using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Mods;
using QuikGraph;
using QuikGraph.Algorithms;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    internal class ModDependencyGraph : AdjacencyGraph<ModDependencyEntry, IEdge<ModDependencyEntry>>, IModDependencyGraph
    {
        public void AddDependency(IMod source, IMod dependency)
        {
            AddVerticesAndEdge(new Edge<ModDependencyEntry>(new ModDependencyEntry(source), new ModDependencyEntry(dependency)));
        }

        public bool HasCycle()
        {
            return !this.IsDirectedAcyclicGraph();
        }

        public IList<ModDependencyEntry> DependenciesOf(ModDependencyEntry sourceEntry)
        {
            return OutEdges(sourceEntry).Select(e => e.Target).ToList();
        }

        public IList<ModDependencyEntry> DependenciesOf(IMod sourceMod)
        {
            return DependenciesOf(new ModDependencyEntry(sourceMod));
        }

        public IEnumerator<ModDependencyEntry> GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}