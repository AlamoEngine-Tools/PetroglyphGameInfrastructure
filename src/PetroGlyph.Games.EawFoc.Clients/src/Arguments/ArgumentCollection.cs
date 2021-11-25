using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <inheritdoc/>
public sealed class ArgumentCollection : IArgumentCollection
{
    /// <summary>
    /// Singleton argument collection, which is always empty.
    /// </summary>
    public static IArgumentCollection Empty = new EmptyArgumentsCollection();

    private readonly IReadOnlyCollection<IGameArgument> _args;

    /// <inheritdoc/>
    public int Count => _args.Count;

    /// <summary>
    /// Creates a new collection from a given list of <see cref="IGameArgument"/>s.
    /// </summary>
    /// <param name="arguments">The arguments to add to this instance.</param>
    public ArgumentCollection(IEnumerable<IGameArgument> arguments)
    {
        _args = new ReadOnlyCollection<IGameArgument>(arguments.ToList());
    }

    /// <inheritdoc/>
    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return _args.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// An always empty argument collection.
    /// </summary>
    private class EmptyArgumentsCollection : IArgumentCollection
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
}