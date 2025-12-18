using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

internal class TestingGameImpl : ITestingGameInstallation
{
    private readonly IFileSystem _fileSystem;
    private readonly IServiceProvider _serviceProvider;

    public IGame Game { get; }

    public TestingGameImpl(IGameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Game = Install(gameIdentity);
    }

    public void InstallDebug()
    {
        if (Game.Platform is not GamePlatform.SteamGold)
            Assert.Fail($"Cannot install Debug files for non-Steam game '{Game}'");
        GameInstallation.CreateFile(_fileSystem, _fileSystem.Path.Combine(Game.Directory.FullName, "StarWarsI.exe"));
    }

    public IDirectoryInfo GetWrongOriginFocRegistryLocation()
    {
        return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(GameInstallation.OriginBasePath, "corruption"));
    }

    public ITestingModInstallation InstallAndAddMod(string name, bool workshop)
    {
        var mod = Game.InstallMod(name, workshop, _serviceProvider);
        Game.AddMod(mod);
        return new TestingModImpl(this, mod, _serviceProvider);
    }

    private IGame Install(IGameIdentity gameIdentity)
    {
        var gameDir = gameIdentity.Type == GameType.Foc
            ? GameInstallation.InstallFoc(_fileSystem, gameIdentity.Platform)
            : GameInstallation.InstallEaw(_fileSystem, gameIdentity.Platform);

        _fileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), _serviceProvider);
        Assert.True(game.Exists());
        return game;
    }
}

