// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

/// <summary>
/// Represents an abstraction for a test installation of a virtual mod of the Petroglyph Star Wars game infrastructure.
/// </summary>
public interface ITestingVirtualModInstallation : ITestingModInstallation
{
    /// <summary>
    /// Gets the virtual mod associated of the test installation.
    /// </summary>
    new IVirtualMod Mod { get; }
}