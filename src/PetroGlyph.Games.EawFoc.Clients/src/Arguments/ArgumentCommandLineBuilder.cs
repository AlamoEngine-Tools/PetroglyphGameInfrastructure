using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

internal class ArgumentCommandLineBuilder : IArgumentCommandLineBuilder
{
    private readonly IArgumentValidator _validator;

    public ArgumentCommandLineBuilder(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _validator = serviceProvider.GetService<IArgumentValidator>() ?? new ArgumentValidator();
    }

    /// <summary>
    /// Converts this collection into a string which can be used as argument sequence for a Petroglyph Star Wars game.
    /// </summary>
    /// <remarks>While this operation sanitizes bad input by throwing an exception, by design, we do not sanity check the values here.</remarks>
    /// <returns>Strings representation of arguments</returns>
    /// <exception cref="GameArgumentException"> This collection contained invalid arguments.
    /// </exception>
    public string BuildCommandLine(IArgumentCollection arguments)
    {
        if (!arguments.Any())
            return string.Empty;

        var argumentBuilder = new StringBuilder();

        foreach (var gameArgument in arguments)
        {
            var validity = _validator.CheckArgument(gameArgument, out var argName, out var argValue);
            if (validity != ArgumentValidityStatus.Valid)
                throw new GameArgumentException(gameArgument, $"Argument is not valid. Reason: {validity}");
            var argumentText = ToCommandLine(gameArgument, argName, argValue);
            argumentBuilder.Append(argumentText);
            argumentBuilder.Append(' ');
        }

        return argumentBuilder.ToString().TrimEnd();
    }

    internal string ToCommandLine(IGameArgument argument, string name, string value)
    {
        switch (argument.Kind)
        {
            case ArgumentKind.Flag:
                return BuildFlagArgument(name, (bool)argument.Value, false);
            case ArgumentKind.DashedFlag:
                return BuildFlagArgument(name, (bool)argument.Value, true);
            case ArgumentKind.KeyValue:
                return BuildKeyValueArgument(name, value);
            case ArgumentKind.ModList:
                if (argument is not IGameArgument<IReadOnlyList<IGameArgument<string>>> modList)
                    throw new GameArgumentException(argument,
                        "Mod List argument is expected to be of type 'IGameArgument<IReadOnlyList<IGameArgument<string>>>'");
                return BuildModListString(modList);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string BuildFlagArgument(string flag, bool value, bool dashed)
    {
        if (!value)
            return string.Empty;
        if (dashed)
            flag = $"-{flag}";
        return flag;
    }

    private static string BuildKeyValueArgument(string key, string value)
    {
        return $"{key}={value}";
    }

    private string BuildModListString(IGameArgument<IReadOnlyList<IGameArgument<string>>> modList)
    {
        var sb = new StringBuilder();
        foreach (var modArg in modList.Value)
        {
            if (modArg.Kind != ArgumentKind.KeyValue)
                throw new GameArgumentException(modArg, "Mod argument must be a key/value argument.");
            var validity = _validator.CheckArgument(modArg, out var key, out var modPath);
            if (validity != ArgumentValidityStatus.Valid)
                throw new GameArgumentException(modArg, $"Mod argument is not valid. Reason: {validity}");
            var argumentText = BuildKeyValueArgument(key, modPath);
            sb.Append(argumentText);
            sb.Append(' ');
        }
        return sb.ToString().TrimEnd();
    }
}