// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

/// <summary>
/// Represents an abstraction for a test installation that provides access
/// to a <see cref="IPlayableObject"/> and its associated <see cref="ITestingGameInstallation"/>.
/// </summary>
public interface ITestingPlayableObjectInstallation
{
    /// <summary>
    /// Gets the game installation associated with this testing playable object installation.
    /// </summary>
    ITestingGameInstallation GameInstallation { get; }

    /// <summary>
    /// Gets the playable object associated with the test installation.
    /// </summary>
    IPlayableObject PlayableObject { get; }
}