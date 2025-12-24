// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace AET.Testing.Attributes;

/// <summary>
/// Represents identifiers for test platforms used to specify platform-specific test execution.
/// </summary>
/// <remarks>
/// This enumeration is used in conjunction with attributes like <see cref="PlatformSpecificFactAttribute"/>
/// and <see cref="PlatformSpecificTheoryAttribute"/> to define tests that should only run on specific platforms.
/// </remarks>
[Flags]
public enum TestPlatformIdentifier
{
    /// <summary>
    /// Represents the Windows platform.
    /// </summary>
    Windows = 1,
    /// <summary>
    /// Represents a Linux platform.
    /// </summary>
    Linux = 2,
}