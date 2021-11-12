using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public sealed class ArgumentCollection : IArgumentCollection
{
    private readonly IReadOnlyCollection<IGameArgument> _args;

    public int Count => _args.Count;

    public ArgumentCollection(IList<IGameArgument> arguments)
    {
        _args = new ReadOnlyCollection<IGameArgument>(arguments);
    }

    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return _args.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}