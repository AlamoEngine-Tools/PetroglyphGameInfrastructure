using System.Collections;
using System.Collections.Generic;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Represents a collection of game arguments used by a Petroglyph Star Wars game.
/// </summary>
public sealed class ArgumentCollection : IReadOnlyCollection<GameArgument>
{
    /// <summary>
    /// Returns an empty argument collection.
    /// </summary>
    public static readonly ArgumentCollection Empty = new();

    private readonly HashSet<GameArgument> _arguments;

    /// <inheritdoc/>
    public int Count => _arguments.Count;

    internal ArgumentCollection(IEnumerable<GameArgument> arguments)
    {
        _arguments = [..arguments];
    }

    private ArgumentCollection()
    {
        _arguments = [];
    }

    /// <inheritdoc/>
    public IEnumerator<GameArgument> GetEnumerator()
    {
        return _arguments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}