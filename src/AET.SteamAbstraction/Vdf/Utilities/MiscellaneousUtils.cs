namespace AET.SteamAbstraction.Vdf.Utilities;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal static class MiscellaneousUtils
{
    public static string ToString(object? value)
    {
        if (value == null)
        {
            return "{null}";
        }

        return value is string ? @"""" + value + @"""" : value.ToString();
    }
}