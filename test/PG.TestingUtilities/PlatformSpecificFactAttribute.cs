﻿using System;
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