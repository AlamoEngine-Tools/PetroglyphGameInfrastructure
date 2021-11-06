using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public sealed class ArgumentCollection : IGameArgumentCollection
{
    private readonly IReadOnlyCollection<IGameArgument> _args;

    public int Count => _args.Count;

    public ArgumentCollection(IList<IGameArgument> arguments)
    {
        _args = new ReadOnlyCollection<IGameArgument>(arguments);
    }

    public string ToCommandlineString()
    {
        // While this operation sanitizes bad input by throwing an exception, by design, we do not sanity check the values here. 
        if (!_args.Any())
            return string.Empty;

        var argumentBuilder = new StringBuilder();

        foreach (var gameArgument in _args)
        {
            var (argName, argValue) = IsValidArgumentOrThrow(gameArgument);
            var argumentText = ArgumentToCommandLine(gameArgument, argName, argValue);
            argumentBuilder.Append(argumentText);
            argumentBuilder.Append(' ');
        }
        
        return argumentBuilder.ToString().TrimEnd();
    }

    private static string ArgumentToCommandLine(IGameArgument gameArgument, string name, string value)
    {
        switch (gameArgument.Kind)
        {
            case ArgumentKind.Flag:
                return name;
            case ArgumentKind.DashedFlag:
                return $"-{name}";
            case ArgumentKind.KeyValue:
                return $"{name}={value}";
            case ArgumentKind.ModList:
                // TODO
                return string.Empty;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static (string name, string value) IsValidArgumentOrThrow(IGameArgument gameArgument)
    {
        var name = gameArgument.Name;
        var value = gameArgument.ValueToCommandLine();

        // We do not support custom arguments
        if (!ArgumentNameCatalog.AllSupportedArgumentNames.Contains(gameArgument.Name))
        {
            name = name.ToUpperInvariant();
            if (!ArgumentNameCatalog.AllSupportedArgumentNames.Contains(gameArgument.Name))
                throw new SecurityException();
        }


        // TODO: Value
        // TODO: Special handle ModList since Value is empty.

        return (name, value);
    }

    public IEnumerator<IGameArgument> GetEnumerator()
    {
        return _args.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static ArgumentCollection Merge(IGameArgumentCollection baseCollection,
        params IGameArgument[] arguments)
    {
        var argumentDict = new Dictionary<string, IGameArgument>();
        var argumentsHashed = new HashSet<IGameArgument>(baseCollection);
        foreach (var arg in baseCollection) 
            argumentDict[arg.Name] = arg;

        foreach (var newArg in arguments)
        {
            if (argumentsHashed.Contains(newArg))
                continue; 
            // We know at this point that the value property cannot be equal to the current existing one.
            // Thus we update the dictionary. 
            argumentDict[newArg.Name] = newArg;
        }

        return new ArgumentCollection(argumentDict.Values.ToList());
    }
}