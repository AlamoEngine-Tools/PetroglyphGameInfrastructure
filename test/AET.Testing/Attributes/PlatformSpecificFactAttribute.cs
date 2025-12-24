// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace AET.Testing.Attributes;

/// <summary>
/// Test attribute that specifies the test should only run on specific platforms.
/// </summary>
/// <remarks>
/// This attribute allows you to define platform-specific tests by specifying the target platforms
/// using <see cref="TestPlatformIdentifier"/>. If the current platform does not match any of the specified
/// platforms, the test will be skipped with an appropriate message.
/// </remarks>
public sealed class PlatformSpecificFactAttribute : FactAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformSpecificFactAttribute"/> class with the specified target platforms.
    /// </summary>
    /// <param name="platformIds">
    /// An array of <see cref="TestPlatformIdentifier"/> values that specify the platforms on which the test should run.
    /// </param>
    /// <remarks>
    /// If the current platform does not match any of the specified <paramref name="platformIds"/>, the test will be skipped
    /// with a message indicating that the test execution is not supported on the current platform.
    /// </remarks>
    public PlatformSpecificFactAttribute(params TestPlatformIdentifier[] platformIds)
    {
        var platforms = platformIds.Select(targetPlatform => OSPlatform.Create(Enum.GetName(typeof(TestPlatformIdentifier), targetPlatform)!.ToUpper()));
        var platformMatches = platforms.Any(RuntimeInformation.IsOSPlatform);

        if (!platformMatches)
            Skip = "Test execution is not supported on the current platform";
    }
}