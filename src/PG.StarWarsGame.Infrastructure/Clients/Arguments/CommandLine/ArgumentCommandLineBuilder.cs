using System.Text;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

internal static class ArgumentCommandLineBuilder
{
    /// <summary>
    /// Converts this collection into a string which can be used as a sequence of arguments for a Petroglyph Star Wars game.
    /// </summary>
    /// <remarks>While this operation sanitizes bad input by throwing an exception, by design, we do not sanity check the values here.</remarks>
    /// <returns>Strings representation of arguments</returns>
    /// <exception cref="GameArgumentException"> <paramref name="arguments"/> has invalid arguments.</exception>
    public static string BuildCommandLine(ArgumentCollection arguments)
    {
        if (arguments.Count == 0)
            return string.Empty;

        var argumentBuilder = new StringBuilder();

        foreach (var gameArgument in arguments)
        {
            var argumentText = ToCommandLine(gameArgument);
            argumentBuilder.Append(argumentText);
            argumentBuilder.Append(' ');
        }

        return argumentBuilder.ToString().TrimEnd();
    }

    private static string ToCommandLine(GameArgument argument)
    {
        if (!argument.IsValid(out var reason))
            throw new GameArgumentException(argument, $"Argument is not valid. Reason: {reason.GetInvalidArgMessage()}");

        switch (argument)
        {
            case FlagArgument flagArgument:
                return BuildFlagArgument(flagArgument);
            case ModArgumentList modList:
                return BuildModListString(modList); 
            default:
                return BuildKeyValueArgument(argument);
        }
    }

    private static string BuildFlagArgument(FlagArgument argument)
    {
        if (!argument.Value)
            return string.Empty;
        return argument.Dashed ? $"-{argument.Name}" : argument.Name;
    }

    private static string BuildKeyValueArgument(GameArgument argument)
    {
        return $"{argument.Name}={argument.ValueToCommandLine()}";
    }

    private static string BuildModListString(ModArgumentList modList)
    {
        var sb = new StringBuilder();
        foreach (var modArg in modList.Value)
        {
            sb.Append(ToCommandLine(modArg));
            sb.Append(' ');
        }
        return sb.ToString().TrimEnd();
    }
}