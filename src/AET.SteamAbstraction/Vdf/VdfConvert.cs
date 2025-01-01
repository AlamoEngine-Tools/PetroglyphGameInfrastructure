using AET.SteamAbstraction.Vdf.Linq;
using System;
using System.IO;

namespace AET.SteamAbstraction.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal static class VdfConvert
{
    public static VProperty Deserialize(TextReader reader)
    {
        return Deserialize(reader, VdfSerializerSettings.Common);
    }

    public static VProperty Deserialize(TextReader reader, VdfSerializerSettings settings)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));
        return new VdfSerializer(settings).Deserialize(reader);
    }
}