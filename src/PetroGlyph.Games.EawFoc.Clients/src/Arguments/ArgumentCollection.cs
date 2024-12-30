using System.Collections;
using System.Collections.Generic;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// 
/// </summary>
public sealed class ArgumentCollection : IReadOnlyCollection<IGameArgument>
{
    /// <summary>
    /// Returns an empty argument collection.
    /// </summary>
    public static readonly ArgumentCollection Empty = new();

    private readonly HashSet<IGameArgument> _arguments;

    /// <inheritdoc/>
    public int Count => _arguments.Count;

    internal ArgumentCollection(IEnumerable<IGameArgument> arguments)
    {
        _arguments = [..arguments];
    }

    private ArgumentCollection()
    {
        _arguments = [];
    }

    /// <inheritdoc/>
    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return _arguments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}