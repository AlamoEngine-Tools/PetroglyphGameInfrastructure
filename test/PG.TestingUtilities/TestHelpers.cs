using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.TestingUtilities;

public static class TestHelpers
{
    private static readonly Random Random = new();

    public static bool RandomBool()
    {
        return Random.Next() % 2 == 0;
    }

    public static long RandomLong()
    {
        var buf = new byte[8];
        Random.NextBytes(buf);
        return BitConverter.ToInt64(buf, 0);
    }

    public static uint RandomUInt()
    {
        return (uint)Random.Next(int.MinValue, int.MaxValue);
    }

    public static ushort RandomUShort()
    {
        return (ushort)Random.Next(ushort.MinValue, ushort.MaxValue);
    }

    public static T GetRandomEnum<T>() where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Next(values.Length))!;
    }

    public static T GetRandom<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        var r = Random.Next(list.Count);
        return list[r];
    }

    public static string ShuffleCasing(string input)
    {
        var characters = input.ToCharArray();

        for (var i = 0; i < characters.Length; i++)
        {
            if (char.IsLetter(characters[i]))
            {
                if (Random.Next(2) == 0)
                {
                    characters[i] = char.IsUpper(characters[i])
                        ? char.ToLower(characters[i])
                        : char.ToUpper(characters[i]);
                }
            }
        }

        return new string(characters);
    }
}