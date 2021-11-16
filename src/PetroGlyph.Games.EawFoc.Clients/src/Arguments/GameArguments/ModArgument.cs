namespace PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;

/// <summary>
/// Dedicated argument to represent a mod
/// </summary>
public sealed class ModArgument : NamedArgument<string>
{
    /// <summary>
    /// Creates a new argument 
    /// </summary>
    /// <param name="value">The absolute or relative path or SteamID of the mod.</param>
    /// <param name="workshops">If <see langword="true"/> this argument will be treated as a Steam Workshops mod.</param>
    public ModArgument(string value, bool workshops) : 
        base(workshops ? ArgumentNameCatalog.SteamModArg : ArgumentNameCatalog.ModPathArg, value, false)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        return Value;
    }
}