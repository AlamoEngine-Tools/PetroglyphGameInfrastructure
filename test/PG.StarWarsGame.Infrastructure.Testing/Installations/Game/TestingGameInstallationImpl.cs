using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using System;
using System.IO.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

internal partial class TestingGameInstallationImpl : ITestingGameInstallation
{
    private readonly IFileSystem _fileSystem;
    private readonly IServiceProvider _serviceProvider;

    public IGame Game { get; }

    public ITestingGameInstallation GameInstallation => this;

    public PlayableModContainer ModContainer => Game as PlayableModContainer;

    public IPlayableObject PlayableObject => Game;

    public TestingGameInstallationImpl(IGameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Game = Install(gameIdentity);
    }

    public void InstallDebug()
    {
        if (Game.Platform is not GamePlatform.SteamGold)
            Assert.Fail($"Cannot install Debug files for non-Steam game '{Game}'");
        GameInstallationHelper.CreateFile(_fileSystem, _fileSystem.Path.Combine(Game.Directory.FullName, "StarWarsI.exe"));
    }

    public IDirectoryInfo GetWrongOriginFocRegistryLocation()
    {
        return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(GameInstallationHelper.OriginBasePath, "corruption"));
    }

    private IGame Install(IGameIdentity gameIdentity)
    {
        var gameDir = gameIdentity.Type == GameType.Foc
            ? GameInstallationHelper.InstallFoc(_fileSystem, gameIdentity.Platform)
            : GameInstallationHelper.InstallEaw(_fileSystem, gameIdentity.Platform);

        _fileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), _serviceProvider);
        Assert.True(game.Exists());
        return game;
    }
}

