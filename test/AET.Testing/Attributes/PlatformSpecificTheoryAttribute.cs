using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace AET.Testing.Attributes;

public sealed class PlatformSpecificTheoryAttribute : TheoryAttribute
{
    public PlatformSpecificTheoryAttribute(params TestPlatformIdentifier[] platformIds)
    {
        var platforms = platformIds.Select(targetPlatform => OSPlatform.Create(Enum.GetName(typeof(TestPlatformIdentifier), targetPlatform)!.ToUpper()));
        var platformMatches = platforms.Any(RuntimeInformation.IsOSPlatform);

        if (!platformMatches)
            Skip = "Test execution is not supported on the current platform";
    }
}