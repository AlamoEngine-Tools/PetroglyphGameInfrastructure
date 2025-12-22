using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

/// <summary>
/// Provides utility methods for creating testing game installations and registries 
/// within the Petroglyph Star Wars game infrastructure testing environment.
/// </summary>
/// <remarks>
/// This class serves as a static entry point for creating instances of 
/// <see cref="ITestingGameInstallation"/> and <see cref="ITestingGameRegistry"/>.
/// It is designed to facilitate testing scenarios by providing test environments of game installations and registries.
/// </remarks>
public static class GameInfrastructureTesting
{
    /// <summary>
    /// Creates a new instance of <see cref="ITestingGameInstallation"/> for the specified <see cref="IGameIdentity"/> and service provider.
    /// </summary>
    /// <param name="gameIdentity">The identity of the game for which the testing installation is being created.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the testing installation.</param>
    /// <returns>An instance of <see cref="ITestingGameInstallation"/> representing the testing installation for the specified game.</returns>
    public static ITestingGameInstallation Game(IGameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        return new TestingGameInstallationImpl(gameIdentity, serviceProvider);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ITestingGameRegistry"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve dependencies required by the registry.</param>
    /// <returns>An instance of <see cref="ITestingGameRegistry"/> that provides methods to create and manage  test game registries.</returns>
    public static ITestingGameRegistry Registry(IServiceProvider serviceProvider)
    {
        return new TestingGameRegistryImpl(serviceProvider);
    }
}