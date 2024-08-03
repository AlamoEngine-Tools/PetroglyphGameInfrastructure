using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Service to build an <see cref="IArgumentCollection"/> which uses the argument's name as unique identifier. Arguments with the same name will be replaced.
/// </summary>
public class UniqueArgumentCollectionBuilder
{
    private readonly Dictionary<string, IGameArgument> _argumentDict = new();

    /// <summary>
    /// Initializes an <see cref="UniqueArgumentCollectionBuilder"/> with no arguments.
    /// </summary>
    public UniqueArgumentCollectionBuilder()
    {
    }

    /// <summary>
    /// Initializes an <see cref="UniqueArgumentCollectionBuilder"/> with a given <paramref name="argumentCollection"/>.
    /// </summary>
    public UniqueArgumentCollectionBuilder(IArgumentCollection argumentCollection)
    {
        AddAll(argumentCollection);
    }

    /// <summary>
    /// Adds or updates an argument to this instance. 
    /// </summary>
    /// <param name="argument">The argument to add or update.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    public UniqueArgumentCollectionBuilder Add(IGameArgument argument)
    {
        if (argument == null) 
            throw new ArgumentNullException(nameof(argument));

        _argumentDict[argument.Name] = argument;
        return this;
    }

    /// <summary>
    /// Removes the argument if present from this instance.
    /// </summary>
    /// <param name="argument">The argument to remove.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    public UniqueArgumentCollectionBuilder Remove(IGameArgument argument)
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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public UniqueArgumentCollectionBuilder Remove(string name)
    {
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
        _argumentDict.Remove(name);
        return this;
    }

    /// <summary>
    /// Adds or updates all arguments from <paramref name="argumentCollection"/> to this instance.
    /// </summary>
    /// <param name="argumentCollection">The arguments to add or update.</param>
    /// <returns>This instance.</returns>
    public UniqueArgumentCollectionBuilder AddAll(IArgumentCollection argumentCollection)
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