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
    public static T GetRandomEnum<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        var random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static T GetRandom<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        var r = new Random().Next(list.Count);
        return list[r];
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