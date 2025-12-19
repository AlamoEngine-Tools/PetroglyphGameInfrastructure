using PG.StarWarsGame.Infrastructure.Games;
using System;
using System.IO.Abstractions;
using AET.Modinfo.Spec;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

internal partial class TestingGameInstallationImpl : TestingModContainerInstallation, ITestingGameInstallation
{
    public IGame Game { get; }

    public override ITestingGameInstallation GameInstallation => this;

    IPhysicalPlayableObject ITestingPhysicalPlayableObjectInstallation.PlayableObject => Game;

    public override IPlayableObject PlayableObject => Game;

    public override PlayableModContainer ModContainer => (Game as PlayableModContainer)!;

    public TestingGameInstallationImpl(IGameIdentity gameIdentity, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Game = Install(gameIdentity);
    }

    public void InstallDebug()
    {
        if (Game.Platform is not GamePlatform.SteamGold)
            Assert.Fail($"Cannot install Debug files for non-Steam game '{Game}'");
        GameInstallationHelper.CreateFile(FileSystem, FileSystem.Path.Combine(Game.Directory.FullName, "StarWarsI.exe"));
    }

    public IDirectoryInfo GetWrongOriginFocRegistryLocation()
    {
        return FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(GameInstallationHelper.OriginBasePath, "corruption"));
    }

    public void InstallLanguage(ILanguageInfo language)
    {
        PlayableObjectTestingUtilities.InstallLanguage(Game, language, FileSystem);
    }

    private IGame Install(IGameIdentity gameIdentity)
    {
        var gameDir = gameIdentity.Type == GameType.Foc
            ? GameInstallationHelper.InstallFoc(FileSystem, gameIdentity.Platform)
            : GameInstallationHelper.InstallEaw(FileSystem, gameIdentity.Platform);

        FileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), ServiceProvider);
        Assert.True(game.Exists());
        return game;
    }
}

