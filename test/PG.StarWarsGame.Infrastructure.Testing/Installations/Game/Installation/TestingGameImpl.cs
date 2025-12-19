using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System;
using System.IO.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Installation;

internal class TestingGameImpl : ITestingGameInstallation
{
    private readonly IFileSystem _fileSystem;
    private readonly IServiceProvider _serviceProvider;

    public IGame Game { get; }

    public ITestingGameInstallation GameInstallation => this;

    public PlayableModContainer ModContainer => Game as PlayableModContainer;

    public IPlayableObject PlayableObject => Game;

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
        GameInstallationHelper.CreateFile(_fileSystem, _fileSystem.Path.Combine(Game.Directory.FullName, "StarWarsI.exe"));
    }

    public IDirectoryInfo GetWrongOriginFocRegistryLocation()
    {
        return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(GameInstallationHelper.OriginBasePath, "corruption"));
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, bool workshop)
    {
        var mod = Game.InstallMod(name, workshop, _serviceProvider);
        Game.AddMod(mod);
        return new TestingPhysicalModImpl(this, mod, _serviceProvider);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo)
    {
        return InstallAndAddMod(modinfo, GITestUtilities.GetRandomWorkshopFlag(Game));
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, bool workshop)
    {
        var mod = Game.InstallMod(workshop, modinfo, _serviceProvider);
        Game.AddMod(mod);
        return new TestingPhysicalModImpl(this, mod, _serviceProvider);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IDirectoryInfo directory, bool workshop, IModinfo modinfo)
    {
        var mod = Game.InstallMod(directory, workshop, modinfo, _serviceProvider);
        Game.AddMod(mod);
        return new TestingPhysicalModImpl(this, mod, _serviceProvider);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, IModDependencyList dependencies)
    {
        var workshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallAndAddMod(name, workshop, dependencies);
    }

    public ITestingVirtualModInstallation AddVirtualMod(string name, ModinfoData modinfo)
    {
        var mod = new VirtualMod(Game, "VirtualModId", modinfo, _serviceProvider);
        Game.AddMod(mod);
        return new TestingVirtualModImpl(this, mod, _serviceProvider);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, bool isWorkshop, IModDependencyList dependencies)
    {
        if (dependencies.Count == 0)
            return InstallAndAddMod(name, isWorkshop);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = dependencies
        };
        return InstallAndAddMod(modinfo, isWorkshop);
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

