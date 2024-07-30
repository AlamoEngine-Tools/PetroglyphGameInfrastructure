using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

internal sealed class ArgumentValidator : IArgumentValidator
{
    private static readonly char[] InvalidArgumentChars = ['\"', '<', '>', '|', ':', '*', '?', '&'];

    public ArgumentValidityStatus CheckArgument(IGameArgument argument, out string name, out string value)
    {
        name = argument.Name;
        value = argument.ValueToCommandLine();
        var kind = argument.Kind;

        // We do not support custom arguments
        if (!ArgumentNameCatalog.AllSupportedArgumentNames.Contains(name))
        {
            name = name.ToUpperInvariant();
            if (!ArgumentNameCatalog.AllSupportedArgumentNames.Contains(name))
                return ArgumentValidityStatus.InvalidName;
        }

        var isNullOrEmpty = string.IsNullOrEmpty(value);

        if (isNullOrEmpty && kind == ArgumentKind.KeyValue)
            return ArgumentValidityStatus.EmptyData;

        // ModList kind is expected to return an empty string.
        if (!isNullOrEmpty && kind == ArgumentKind.ModList)
            return ArgumentValidityStatus.InvalidData;

        var invalidChars = InvalidArgumentChars;
        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
                return ArgumentValidityStatus.PathContainsSpaces;
            
            // 0..31 are ASCII control characters
            if (c <= 31 || invalidChars.Contains(c))
                return ArgumentValidityStatus.IllegalCharacter;
        }
        return ArgumentValidityStatus.Valid;
    }
}