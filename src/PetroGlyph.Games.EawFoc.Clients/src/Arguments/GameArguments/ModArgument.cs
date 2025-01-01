using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Dedicated argument to represent a mod.
/// </summary>
/// <remarks>
/// Use <see cref="GameArgumentsBuilder.AddSingleMod(IPhysicalMod)"/> and overloads or
/// <see cref="GameArgumentsBuilder.AddMods(System.Collections.Generic.IList{IMod})"/> and overloads to add mods to your game arguments.</remarks>
public sealed class ModArgument : NamedArgument<IDirectoryInfo>
{
    internal readonly IDirectoryInfo GameDir;
    private readonly bool _isWorkshop;

    internal override bool HasPathValue => !_isWorkshop;

    // We do not use the game property of the mod, so that users can specify any other game as base if they really like to
    internal ModArgument(IDirectoryInfo modDir, IDirectoryInfo gameDirectory, bool isWorkshops) 
        : base(isWorkshops ? GameArgumentNames.SteamModArg : GameArgumentNames.ModPathArg, modDir, false)
    {
        _isWorkshop = isWorkshops;
        GameDir = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));
    }

    internal override string ValueToCommandLine()
    {
        return _isWorkshop ? ArgumentValueSerializer.Serialize(Value.Name) : ArgumentValueSerializer.ShortenPath(Value, GameDir);
    }

    /// <summary>
    /// Checks whether the given value is a valid SteamID.
    /// </summary>
    /// <remarks>Path checking is already completed if this method is invoked.</remarks>
    private protected override bool IsDataValid()
    {
        return !_isWorkshop || ulong.TryParse(Value.Name, out _);
    }
}