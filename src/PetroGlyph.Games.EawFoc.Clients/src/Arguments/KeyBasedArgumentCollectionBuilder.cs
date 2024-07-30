using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Service to build an <see cref="IArgumentCollection"/> which takes the argument's name as key.
/// <para>Adding new arguments will update an existing, if an argument with the same name is already present.</para>
/// </summary>
public class KeyBasedArgumentCollectionBuilder
{
    private readonly Dictionary<string, IGameArgument> _argumentDict = new();

    /// <summary>
    /// Initializes an <see cref="KeyBasedArgumentCollectionBuilder"/> with no arguments.
    /// </summary>
    public KeyBasedArgumentCollectionBuilder()
    {
    }

    /// <summary>
    /// Initializes an <see cref="KeyBasedArgumentCollectionBuilder"/> with a given <paramref name="argumentCollection"/>.
    /// </summary>
    public KeyBasedArgumentCollectionBuilder(IArgumentCollection argumentCollection)
    {
        AddAll(argumentCollection);
    }

    /// <summary>
    /// Adds or updates an argument to the this instance. 
    /// </summary>
    /// <param name="argument">The argument to add or update.</param>
    /// <returns>This instance.</returns>
    public KeyBasedArgumentCollectionBuilder Add(IGameArgument argument)
    {
        if (argument == null) 
            throw new ArgumentNullException(nameof(argument));

        _argumentDict[argument.Name] = argument;
        return this;
    }

    /// <inheritdoc/>
    public KeyBasedArgumentCollectionBuilder Remove(IGameArgument argument)
    {
        if (argument == null)
            throw new ArgumentNullException(nameof(argument));

        return Remove(argument.Name);
    }

    /// <summary>
    /// Removes the argument with the given <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the argument to remove.</param>
    /// <returns>This instance.</returns>
    public KeyBasedArgumentCollectionBuilder Remove(string name)
    {
        _argumentDict.Remove(name);
        return this;
    }

    /// <summary>
    /// Adds or updates all arguments from <paramref name="argumentCollection"/> to this instance.
    /// </summary>
    /// <param name="argumentCollection">The arguments to add or update.</param>
    /// <returns>This instance.</returns>
    public KeyBasedArgumentCollectionBuilder AddAll(IArgumentCollection argumentCollection)
    {
        foreach (var arg in argumentCollection)
            Add(arg);
        return this;
    }

    /// <summary>
    /// Creates an <see cref="IArgumentCollection"/> from this instance.
    /// </summary>
    /// <returns>The created <see cref="IArgumentCollection"/>.</returns>
    public IArgumentCollection Build()
    {
        return new ArgumentCollection(_argumentDict.Values.ToList());
    }
}