using System.Collections.Generic;

namespace AET.SteamAbstraction.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal class VdfSerializerSettings
{
    public static VdfSerializerSettings Default => new();
    public static VdfSerializerSettings Common => new()
    {
        UsesEscapeSequences = true,
        UsesConditionals = false
    };

    public bool UsesEscapeSequences;

    public bool UsesConditionals = true;

    public IReadOnlyList<string>? DefinedConditionals { get; set; }

    public int MaximumTokenSize = 4096;
}