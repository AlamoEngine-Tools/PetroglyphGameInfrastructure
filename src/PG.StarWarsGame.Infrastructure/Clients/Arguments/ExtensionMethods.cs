using System;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

internal static class ExtensionMethods
{
    public static string GetInvalidArgMessage(this ArgumentValidityStatus status)
    {
        switch (status)
        {
            case ArgumentValidityStatus.Valid:
                return "The argument is valid";
            case ArgumentValidityStatus.EmptyData:
                return "The argument does not contain a value.";
            case ArgumentValidityStatus.InvalidName:
                return "The name of the argument is not supported by a Petroglyph Star Wars game";
            case ArgumentValidityStatus.IllegalCharacter:
                return "The argument contains an illegal character.";
            case ArgumentValidityStatus.PathContainsSpaces:
                return "The argument is a file or directory path and contains a space character. Spaces (as well as quoted spaces) are not supported.";
            case ArgumentValidityStatus.InvalidData:
                return "The value of the argument is not valid.";
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}