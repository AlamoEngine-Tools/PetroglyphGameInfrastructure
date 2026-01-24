// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Represents a fake Steam process for testing purposes.
/// </summary>
public interface ISteamFakeProcess
{
    /// <summary>
    /// Terminates the associated process.
    /// </summary>
    void Kill();
}