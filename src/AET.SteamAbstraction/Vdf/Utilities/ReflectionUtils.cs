using System;

namespace AET.SteamAbstraction.Vdf.Utilities;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

internal static class ReflectionUtils
{
    public static bool IsNullableType(Type t)
    {
        if (t == null)
            throw new ArgumentNullException(nameof(t));
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}