using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <inheritdoc/>
public sealed class ArgumentCollection : IArgumentCollection
{
    private readonly IReadOnlyCollection<IGameArgument> _args;

    /// <inheritdoc/>
    public int Count => _args.Count;

    /// <summary>
    /// Creates a new collection from a given list of <see cref="IGameArgument"/>s.
    /// </summary>
    /// <param name="arguments">The arguments to add to this instance.</param>
    public ArgumentCollection(IList<IGameArgument> arguments)
    {
        _args = new ReadOnlyCollection<IGameArgument>(arguments);
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
}