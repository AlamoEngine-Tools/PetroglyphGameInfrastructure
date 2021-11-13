namespace PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;

public sealed class ModArgument : NamedArgument<string>
{
    public ModArgument(string value, bool workshops) : 
        base(workshops ? ArgumentNameCatalog.SteamModArg : ArgumentNameCatalog.ModPathArg, value, false)
    {
    }

    public override string ValueToCommandLine()
    {
        return Value;
    }
}