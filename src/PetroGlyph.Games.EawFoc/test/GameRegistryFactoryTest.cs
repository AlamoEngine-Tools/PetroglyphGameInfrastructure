using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameRegistryFactoryTest
{
    private readonly GameRegistryFactory _service;
    private readonly IRegistry _registry = new InMemoryRegistry();
    private readonly IFileSystem _fileSystem = new MockFileSystem();


    public GameRegistryFactoryTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(_fileSystem);
        sc.AddSingleton(_registry);
        _service = new GameRegistryFactory(sc.BuildServiceProvider());
    }

    [Theory]
    [InlineData(GameType.Eaw, null, false)]
    [InlineData(GameType.Foc, null, false)]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath, true)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath, true)]
    [InlineData(GameType.Eaw, "someRandomPathEaw", false)]
    [InlineData(GameType.Foc, "someRandomPathFoc", false)]
    // Swapped subPaths
    [InlineData(GameType.Eaw, GameRegistryFactory.FocRegistryPath, false)]
    [InlineData(GameType.Foc, GameRegistryFactory.EawRegistryPath, false)]
    public void CreateRegistry_RegistryCreated(GameType gameType, string? subPath, bool isSubPathValid)
    {
        if (subPath is not null)
        {
            using var lm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var k = lm.CreateSubKey(subPath);
        }
        var gameRegistry = _service.CreateRegistry(gameType);
        Assert.NotNull(gameRegistry);
        Assert.Equal(gameType, gameRegistry.Type);
        Assert.Equal(isSubPathValid, gameRegistry.Exits);
    }
}