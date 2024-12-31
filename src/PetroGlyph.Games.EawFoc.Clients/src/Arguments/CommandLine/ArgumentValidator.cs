using System;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

internal static class ArgumentValidator
{
    private static readonly IFileSystem FileSystem;

    static ArgumentValidator()
    {
        FileSystem = new FileSystem();
    }

    // Based on: https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/07-Input_Validation_Testing/12-Testing_for_Command_Injection
    // Additionally, the game does not like quotes and spaces, so we filter these out too.
    // NB: Double Colon ':' is checked separately as we want to allow absolute paths on Windows
    private static readonly char[] InvalidArgumentChars =
        ['\"', '\'', '#', '+', ',', '`', '<', '>', '|', ';', '*', '?', '&', ' ', '!', '$', '=', '@', '%', '~'];

    public static ArgumentValidityStatus CheckArgument(GameArgument argument)
    { 
        // We do not support custom arguments
        if (!GameArgumentNames.AllInternalSupportedArgumentNames.Contains(argument.Name))
            return ArgumentValidityStatus.InvalidName;

        var value = argument.ValueToCommandLine();

        return argument.Kind switch
        {
            ArgumentKind.ModList => string.IsNullOrEmpty(value)
                ? ArgumentValidityStatus.Valid
                : ArgumentValidityStatus.InvalidData,
            ArgumentKind.KeyValue when string.IsNullOrEmpty(value) => ArgumentValidityStatus.EmptyData,
            _ => ContainsInvalidCharacters(value.AsSpan(), argument.HasPathValue)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ArgumentValidityStatus ContainsInvalidCharacters(ReadOnlySpan<char> value, bool isPathValue)
    {
        var invalidChars = InvalidArgumentChars;
        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
                return ArgumentValidityStatus.PathContainsSpaces;

            // 0..31 are ASCII control characters
            if (c <= 31 || invalidChars.Contains(c))
                return ArgumentValidityStatus.IllegalCharacter;
        }

        if (!CheckDoubleColon(value, isPathValue))
            return ArgumentValidityStatus.IllegalCharacter;

        return ArgumentValidityStatus.Valid;
    }

    private static bool CheckDoubleColon(ReadOnlySpan<char> value, bool isPathValue)
    {
        var dColonIndex = value.LastIndexOf(':');
        if (dColonIndex == -1)
            return true;
        if (!isPathValue)
            return false;
        var root = FileSystem.Path.GetPathRoot(value);
        var rootColonIndex = root.IndexOf(':');
        return rootColonIndex != -1 && rootColonIndex == dColonIndex;
    }
}