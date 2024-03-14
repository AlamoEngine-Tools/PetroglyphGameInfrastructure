using System;
using System.Collections.Generic;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// <see cref="IArgumentCollectionBuilder"/> which takes the argument's name as key.
/// <para>Adding new arguments will update an existing, if an argument with the same name is already present.</para>
/// </summary>
public class KeyBasedArgumentCollectionBuilder : IArgumentCollectionBuilder
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
    public IArgumentCollectionBuilder Add(IGameArgument argument)
    {
        if (argument == null) 
            throw new ArgumentNullException(nameof(argument));

        _argumentDict[argument.Name] = argument;
        return this;
    }

    /// <inheritdoc/>
    public IArgumentCollectionBuilder Remove(IGameArgument argument)
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
    public IArgumentCollectionBuilder Remove(string name)
    {
        _argumentDict.Remove(name);
        return this;
    }

    /// <summary>
    /// Adds or updates all arguments from <paramref name="argumentCollection"/> to this instance.
    /// </summary>
    /// <param name="argumentCollection">The arguments to add or update.</param>
    /// <returns>This instance.</returns>
    public IArgumentCollectionBuilder AddAll(IArgumentCollection argumentCollection)
    {
        foreach (var arg in argumentCollection)
            Add(arg);
        return this;
    }

    /// <inheritdoc/>
    public IArgumentCollection Build()
    {
        return new ArgumentCollection(_argumentDict.Values.ToList());
    }
}