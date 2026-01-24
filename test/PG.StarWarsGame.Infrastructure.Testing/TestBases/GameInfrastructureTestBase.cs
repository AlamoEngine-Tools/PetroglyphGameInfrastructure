// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

/// <summary>
/// Provides a base class for testing infrastructure components of the Petroglyph Star Wars game test framework.
/// This class facilitates the setup of services, file system management, and game or mod installations
/// required for testing purposes.
/// </summary>
public abstract class GameInfrastructureTestBase : TestBaseWithFileSystem
{
    private ITestingGameInstallation? _gameInstallation;

    /// <summary>
    /// Configures test services by adding them to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to which services will be added.</param>
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        PetroglyphGameInfrastructure.InitializeServices(serviceCollection);
    }

    /// <summary>
    /// Retrieves an existing game installation or creates a new one if it does not already exist.
    /// </summary>
    /// <param name="identity">The optional identity of the game. If not provided, a random game identity will be used.</param>
    /// <returns>An instance of <see cref="ITestingGameInstallation"/> representing the game installation.</returns>
    protected virtual ITestingGameInstallation GetOrCreateGameInstallation(IGameIdentity? identity = null)
    { 
        if (_gameInstallation is not null)
            return _gameInstallation;
        identity ??= GITestUtilities.GetRandomGameIdentity(realOnly: true);
        return _gameInstallation = GameInfrastructureTesting.Game(identity, ServiceProvider);
    }

    /// <summary>
    /// Installs a mod with the specified name and dependencies into the game installation and adds it to the <see cref="IModContainer.Mods"/> collection of the game.
    /// </summary>
    /// <param name="name">The name of the mod to be installed.</param>
    /// <param name="layout">Specifies the dependency resolution layout to be used. Defaults to <see cref="DependencyResolveLayout.FullResolved"/>.</param>
    /// <param name="deps">A collection of mod dependencies to be resolved and added.</param>
    /// <returns>An instance of <see cref="ITestingPhysicalModInstallation"/> representing the installed mod.</returns>
    protected ITestingPhysicalModInstallation InstallAndAddModWithDependencies(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        return GetOrCreateGameInstallation().InstallAndAddMod(name, new DependencyList(deps, layout));
    }
}