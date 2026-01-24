// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

/// <summary>
/// Represents an abstraction for a test installation that provides access to a <see cref="PlayableModContainer"/>.
/// </summary>
public interface ITestingModContainerInstallation : ITestingPlayableObjectInstallation
{
    /// <summary>
    /// Gets the <see cref="PlayableModContainer"/> associated with this installation.
    /// </summary>
    PlayableModContainer ModContainer { get; }
}