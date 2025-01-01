namespace AET.SteamAbstraction.Vdf.Linq;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal enum VTokenType
{
    None = 0,
    Property = 1,
    Object = 2,
    Value = 3,
    Comment = 4,
    Conditional = 5
}