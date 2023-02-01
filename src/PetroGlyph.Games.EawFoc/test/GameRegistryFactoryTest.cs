using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test;

public class GameRegistryFactoryTest
{
    private readonly GameRegistryFactory _service;

    public GameRegistryFactoryTest()
    {
        _service = new GameRegistryFactory();
    }

    [Fact]
    public void TestNotFound()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IRegistry>(new InMemoryRegistry());
        Assert.Throws<GameRegistryNotFoundException>(() =>
            _service.CreateRegistry(GameType.EaW, sc.BuildServiceProvider()));
    }

    [Fact]
    public void TestEaWRegistryFound()
    {
        var sc = new ServiceCollection();
        var registry = new InMemoryRegistry();
        var lm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        lm.CreateSubKey(GameRegistryFactory.EawRegistryPath);

        sc.AddSingleton<IRegistry>(registry);
        var gameRegistry = _service.CreateRegistry(GameType.EaW, sc.BuildServiceProvider());
        Assert.NotNull(gameRegistry);
        Assert.Equal(GameType.EaW, gameRegistry.Type);
    }

    [Fact]
    public void TestFocRegistryFound()
    {
        var sc = new ServiceCollection();
        var registry = new InMemoryRegistry();
        var lm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        lm.CreateSubKey(GameRegistryFactory.FocRegistryPath);

        sc.AddSingleton<IRegistry>(registry);
        var gameRegistry = _service.CreateRegistry(GameType.Foc, sc.BuildServiceProvider());
        Assert.NotNull(gameRegistry);
        Assert.Equal(GameType.Foc, gameRegistry.Type);
    }
}