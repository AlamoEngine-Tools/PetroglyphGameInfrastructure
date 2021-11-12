using System.Collections;
using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public struct EmptyArgumentsCollection : IArgumentCollection
{
    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return new Enumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 0;

    public struct Enumerator : IEnumerator<IGameArgument>
    {
        public bool MoveNext() => false;

        public void Reset() { }

        public IGameArgument Current => null!;

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}