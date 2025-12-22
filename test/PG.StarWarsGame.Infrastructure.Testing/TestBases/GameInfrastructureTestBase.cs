using System;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.TestBases;

/// <summary>
/// Provides a base class for testing infrastructure components of the Petroglyph Star Wars game test framework.
/// This class facilitates the setup of services, file system management, and game or mod installations
/// required for testing purposes.
/// </summary>
public abstract class GameInfrastructureTestBase : TestBaseWithServiceProvider
{
    private ITestingGameInstallation? _gameInstallation;

    /// <summary>
    /// Gets the file system abstraction used for testing purposes.
    /// This property provides access to an <see cref="IFileSystem"/> instance, which is lazily initialized
    /// and can be overridden by derived classes to customize the file system behavior.
    /// </summary>
    /// <remarks>
    /// The file system is initialized using the <see cref="CreateFileSystem"/> method. If the initialization
    /// fails or returns <c>null</c>, an <see cref="InvalidOperationException"/> is thrown.
    /// </remarks>
    [field:MaybeNull, AllowNull]
    protected IFileSystem FileSystem => LazyInitializer.EnsureInitialized(ref field, CreateFileSystem)
                                        ?? throw new InvalidOperationException("Creation of file system must not return null.");
    
    /// <summary>
    /// Creates and returns a new instance of the file system abstraction for testing purposes.
    /// </summary>
    /// <remarks>
    /// This method is invoked to initialize the <see cref="FileSystem"/> property. By default, it returns
    /// a <see cref="MockFileSystem"/> instance, but derived classes can override this method to provide
    /// a custom implementation of <see cref="IFileSystem"/>.
    /// </remarks>
    /// <returns>
    /// An instance of <see cref="IFileSystem"/> representing the file system abstraction to be used in tests.
    /// </returns>
    protected virtual IFileSystem CreateFileSystem()
    {
        return new MockFileSystem();
    }

    /// <summary>
    /// Configures test services by adding them to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to which services will be added.</param>
    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton(FileSystem);
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