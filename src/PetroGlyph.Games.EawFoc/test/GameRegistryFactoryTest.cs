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

    [Fact]
    public void TestNotFound()
    {
        Assert.Throws<GameRegistryNotFoundException>(() =>
            _service.CreateRegistry(GameType.Eaw));
    }

    [Fact]
    public void TestEaWRegistryFound()
    { 
        var lm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        lm.CreateSubKey(GameRegistryFactory.EawRegistryPath);

        var gameRegistry = _service.CreateRegistry(GameType.Eaw);
        Assert.NotNull(gameRegistry);
        Assert.Equal(GameType.Eaw, gameRegistry.Type);
    }

    [Fact]
    public void TestFocRegistryFound()
    {
        var lm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        lm.CreateSubKey(GameRegistryFactory.FocRegistryPath);

        var gameRegistry = _service.CreateRegistry(GameType.Foc);
        Assert.NotNull(gameRegistry);
        Assert.Equal(GameType.Foc, gameRegistry.Type);
    }
}