using System.Collections;
using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// An always empty argument collection.
/// </summary>
public struct EmptyArgumentsCollection : IArgumentCollection
{
    /// <inheritdoc/>
    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return new Enumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public int Count => 0;

    private struct Enumerator : IEnumerator<IGameArgument>
    {
        public bool MoveNext() => false;

        public void Reset() { }

        public IGameArgument Current => null!;

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}