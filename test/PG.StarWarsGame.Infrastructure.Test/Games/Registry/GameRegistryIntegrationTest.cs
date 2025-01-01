using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Games.Registry;

public class GameRegistryIntegrationTest
{
    private readonly IRegistry _registry = new WindowsRegistry();
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;

    public GameRegistryIntegrationTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton(_registry);
        _serviceProvider = sc.BuildServiceProvider();
    }


    [PlatformSpecificTheory(TestPlatformIdentifier.Windows)]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void IntegrationTest(GameType gameType, string gameRegistryPath)
    {
        RunIntegrationTest(gameType, gameRegistryPath,
            assertAction: registry =>
            {
                Assert.Equal(gameType, registry.Type);
                Assert.True(registry.Exits);
                Assert.Equal(new Version(1, 0), registry.Version);
                Assert.True(registry.Installed);
            },
            createRegistryAction: regKey =>
            {
                using var vKey = regKey.CreateSubKey("1.0");
                vKey!.SetValue("Installed", 1);
            }
        );
    }

    private void RunIntegrationTest(
        GameType gameType, 
        string gameRegistryPath,
        Action<IGameRegistry> assertAction, 
        Action<IRegistryKey> createRegistryAction)
    {
        var factory = new GameRegistryFactory(_serviceProvider);
        using var registry = factory.CreateRegistry(gameType);

        IRegistryKey? underlyingRegistry = null;
        var existed = registry.Exits;

        try
        {
            // In the case the registry already exists
            if (!existed)
            {
                underlyingRegistry = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .CreateSubKey(gameRegistryPath);
                if (underlyingRegistry is null)
                    Assert.Fail("Unable to create underlying registry key.");
                createRegistryAction(underlyingRegistry);
            }
            assertAction(registry);
        }
        finally
        {
            if (!existed)
            {
                try
                {
                    underlyingRegistry?.DeleteKey(string.Empty, true);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
            underlyingRegistry?.Dispose();
        }
    }
}