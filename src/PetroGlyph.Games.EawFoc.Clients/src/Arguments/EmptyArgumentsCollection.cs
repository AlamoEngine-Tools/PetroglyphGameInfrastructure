using System.Collections;
using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public struct EmptyArgumentsCollection : IGameArgumentCollection
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

    public string ToCommandlineString() => string.Empty;

    private struct Enumerator : IEnumerator<IGameArgument>
    {
        public IGameArgument Current => default!;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }
        
        public void Dispose()
        {
        }
    }
}