using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PG.TestingUtilities;

public class PlatformSpecificFactAttribute : FactAttribute
{
    public PlatformSpecificFactAttribute(params TestPlatformIdentifier[] platformIds)
    {
        var platforms = platformIds.Select(targetPlatform => OSPlatform.Create(Enum.GetName(typeof(TestPlatformIdentifier), targetPlatform)!.ToUpper()));
        var platformMatches = platforms.Any(RuntimeInformation.IsOSPlatform);

        if (!platformMatches)
            Skip = "Test execution is not supported on the current platform";
    }
}

public class PlatformSpecificTheoryAttribute : TheoryAttribute
{
    public PlatformSpecificTheoryAttribute(params TestPlatformIdentifier[] platformIds)
    {
        var platforms = platformIds.Select(targetPlatform => OSPlatform.Create(Enum.GetName(typeof(TestPlatformIdentifier), targetPlatform)!.ToUpper()));
        var platformMatches = platforms.Any(RuntimeInformation.IsOSPlatform);

        if (!platformMatches)
            Skip = "Test execution is not supported on the current platform";
    }
}


[Flags]
public enum TestPlatformIdentifier
{
    Windows = 1,
    Linux = 2,
}

[StructLayout(LayoutKind.Explicit)]
public struct EmptyStruct;

public static class TestHelpers
{
    private static Random random = new();

    public static T GetRandomEnum<T>() where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static T GetRandom<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        var r = random.Next(list.Count);
        return list[r];
    }

    public static string ShuffleCasing(string input)
    {
        var characters = input.ToCharArray();

        for (var i = 0; i < characters.Length; i++)
        {
            if (char.IsLetter(characters[i]))
            {
                if (random.Next(2) == 0)
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

public class ExceptionTest
{
    public static void AssertException(Exception e,
        Exception? innerException = null,
        string? message = null,
        string? stackTrace = null,
        bool validateMessage = true)
    {
        Assert.Equal(innerException, e.InnerException);
        if (validateMessage)
            Assert.Equal(message, e.Message);
        else
            Assert.NotNull(e.Message);
        Assert.Equal(stackTrace, e.StackTrace);
    }
}