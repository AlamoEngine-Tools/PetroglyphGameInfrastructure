namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public sealed class ModArgument : NamedArgument<string>
{
    public ModArgument(string value, bool workshops) : 
        base(workshops ? "STEAMMOD" : "MODPATH", value, false)
    {
    }

    public override string ValueToCommandLine()
    {
        return Value;
    }
}