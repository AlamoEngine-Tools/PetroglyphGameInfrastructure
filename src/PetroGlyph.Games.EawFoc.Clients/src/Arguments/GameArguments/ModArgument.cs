namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Dedicated argument to represent a mod
/// </summary>
public sealed class ModArgument : NamedArgument<string>
{
    private readonly bool _workshops;

    /// <summary>
    /// Creates a new argument 
    /// </summary>
    /// <param name="value">The absolute or relative path or SteamID of the mod.</param>
    /// <param name="workshops">If <see langword="true"/> this argument will be treated as a Steam Workshops mod.</param>
    public ModArgument(string value, bool workshops) : 
        base(workshops ? ArgumentNameCatalog.SteamModArg : ArgumentNameCatalog.ModPathArg, value, false)
    {
        _workshops = workshops;
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return Value;
    }

    /// <summary>
    /// Checks whether the given value is a valid SteamID.
    /// </summary>
    /// <remarks>Path checking is already completed if this method is invoked.</remarks>
    protected override bool IsDataValid()
    {
        return !_workshops || ulong.TryParse(Value, out _);
    }
}