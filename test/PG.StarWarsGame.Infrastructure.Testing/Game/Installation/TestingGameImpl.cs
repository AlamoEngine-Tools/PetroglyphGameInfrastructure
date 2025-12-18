using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

internal class TestingGameImpl(IServiceProvider serviceProvider) : ITestingGameInstallation
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public IGame? Game { get; private set; }

    [MemberNotNull(nameof(Game))]
    public IGame Install(IGameIdentity gameIdentity)
    {
        ThrowIfInstalled();
        var gameDir = gameIdentity.Type == GameType.Foc
            ? GameInstallation.InstallFoc(_fileSystem, gameIdentity.Platform)
            : GameInstallation.InstallEaw(_fileSystem, gameIdentity.Platform);

        _fileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), serviceProvider);
        Assert.True(game.Exists());
        return Game = game;
    }

    public IGame InstallRandom()
    {
        return Install(GITestUtilities.GetRandomGameIdentity(realOnly: true));
    }

    public void InstallDebug()
    {
        ThrowIfNotInstalled();
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
        ThrowIfNotInstalled();
        var mod = Game.InstallMod(name, workshop, serviceProvider);
        Game.AddMod(mod);
        return new TestingModImpl(this, mod, serviceProvider);
    }


    [MemberNotNull(nameof(Game))]
    private void ThrowIfNotInstalled()
    {
        if (Game is null)
            throw new InvalidOperationException("Game not installed");
    }

    private void ThrowIfInstalled()
    {
        if (Game is not null)
            throw new InvalidOperationException("Game already installed for this testing instance. Create a new one.");
    }
}

