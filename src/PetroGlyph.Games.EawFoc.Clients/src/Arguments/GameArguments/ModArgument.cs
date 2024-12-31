namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Dedicated argument to represent a mod.
/// </summary>
/// <remarks>
/// Use <see cref="GameArgumentsBuilder.AddSingleMod"/> or
/// <see cref="GameArgumentsBuilder.AddMods"/> to add mods to your game arguments.</remarks>
public sealed class ModArgument : NamedArgument<string>
{
    private readonly bool _workshops;

    internal override bool HasPathValue => !_workshops;

    internal ModArgument(string value, bool workshops) 
        : base(workshops ? GameArgumentNames.SteamModArg : GameArgumentNames.ModPathArg, value, false)
    {
        _workshops = workshops;
    }

    /// <summary>
    /// Checks whether the given value is a valid SteamID.
    /// </summary>
    /// <remarks>Path checking is already completed if this method is invoked.</remarks>
    private protected override bool IsDataValid()
    {
        return !_workshops || ulong.TryParse(Value, out _);
    }
}