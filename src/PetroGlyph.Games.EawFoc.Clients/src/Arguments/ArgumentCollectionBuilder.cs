using System.Collections.Generic;
using System.Linq;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public class ArgumentCollectionBuilder
{
    private readonly Dictionary<string, IGameArgument> _argumentDict = new();

    public ArgumentCollectionBuilder()
    {
    }

    public ArgumentCollectionBuilder(IArgumentCollection argumentCollection)
    {
        AddAll(argumentCollection);
    }

    public ArgumentCollectionBuilder Add(IGameArgument argument)
    {
        Requires.NotNull(argument, nameof(argument));
        _argumentDict[argument.Name] = argument;
        return this;
    }

    public ArgumentCollectionBuilder Remove(IGameArgument argument)
    {
        Requires.NotNull(argument, nameof(argument));
        return Remove(argument.Name);
    }

    public ArgumentCollectionBuilder Remove(string argument)
    {
        _argumentDict.Remove(argument);
        return this;
    }

    public ArgumentCollectionBuilder AddAll(IArgumentCollection argumentCollection)
    {
        foreach (var arg in argumentCollection)
            Add(arg);
        return this;
    }
    
    public IArgumentCollection Build()
    {
        return new ArgumentCollection(_argumentDict.Values.ToList());
    }
}