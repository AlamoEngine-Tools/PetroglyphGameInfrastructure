﻿using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameRegistryFactoryTest
{
    private readonly GameRegistryFactory _service = new();
    private readonly IFileSystem _fileSystem = new MockFileSystem();

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
        sc.AddSingleton(_fileSystem);
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
        sc.AddSingleton(_fileSystem);
        var registry = new InMemoryRegistry();
        var lm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        lm.CreateSubKey(GameRegistryFactory.FocRegistryPath);

        sc.AddSingleton<IRegistry>(registry);
        var gameRegistry = _service.CreateRegistry(GameType.Foc, sc.BuildServiceProvider());
        Assert.NotNull(gameRegistry);
        Assert.Equal(GameType.Foc, gameRegistry.Type);
    }
}