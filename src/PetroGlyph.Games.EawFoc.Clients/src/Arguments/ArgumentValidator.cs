using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

internal sealed class ArgumentValidator : IArgumentValidator
{

    // Based on: https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/07-Input_Validation_Testing/12-Testing_for_Command_Injection
    // Additionally, the game does not like quotes and spaces, so we filter these out too.
    private static readonly char[] InvalidArgumentChars = ['\"', '\'', '#', '+', ',', '`', '<', '>', '|', ':', ';', '*', '?', '&', ' ', '!', '$', '=', '@', '%', '~'];

    public ArgumentValidityStatus CheckArgument(IGameArgument argument, out string name, out string value)
    {
        name = argument.Name.ToUpperInvariant();
        value = argument.ValueToCommandLine();
        var kind = argument.Kind;

        // We do not support custom arguments
        if (!ArgumentNameCatalog.AllSupportedArgumentNames.Contains(name))
            return ArgumentValidityStatus.InvalidName;

        var isNullOrEmpty = string.IsNullOrEmpty(value);

        if (isNullOrEmpty && kind == ArgumentKind.KeyValue)
            return ArgumentValidityStatus.EmptyData;

        // ModList must always return an empty string.
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