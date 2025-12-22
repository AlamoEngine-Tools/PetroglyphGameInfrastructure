using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using System;
using System.IO.Abstractions;
using Xunit;
using RegistryHive = AnakinRaW.CommonUtilities.Registry.RegistryHive;
using RegistryView = AnakinRaW.CommonUtilities.Registry.RegistryView;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

internal sealed class TestingGameRegistryImpl(IServiceProvider serviceProvider) : ITestingGameRegistry
{
    private readonly IGameRegistryFactory _registryFactory = serviceProvider.GetRequiredService<IGameRegistryFactory>();
    private readonly IRegistry _registry = serviceProvider.GetRequiredService<IRegistry>();

    public IGameRegistry CreateNonExistingRegistry(GameType gameType)
    {
        return _registryFactory.CreateRegistry(gameType);
    }

    public IGameRegistry CreateInstalled(IGame game)
    {
        return CreateFrom(TestGameRegistrySetupData.Installed(game.Type, game.Directory));
    }

    public IGameRegistry CreateFrom(TestGameRegistrySetupData registrySetupData)
    {
        var gameRegistry = _registryFactory.CreateRegistry(registrySetupData.GameType);
        InitializeRegistry(registrySetupData, null);
        return gameRegistry;
    }

    private void InitializeRegistry(TestGameRegistrySetupData setupData, IDirectoryInfo? customDirectoryInfo)
    {
        var gameKeyPath = setupData.GameType == GameType.Eaw ? GameRegistryFactory.EawRegistryPath : GameRegistryFactory.FocRegistryPath;

        using var hklm = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        hklm.DeleteKey(gameKeyPath, true);

        if (!setupData.CreateRegistry)
            return;

        using var gameKey = hklm.CreateSubKey(gameKeyPath);

        if (!setupData.InitRegistry)
            return;

        Assert.NotNull(gameKey);
        using var versionedKey = gameKey.CreateSubKey(GameRegistry.VersionKey);
        Assert.NotNull(versionedKey);

        versionedKey.SetValue(GameRegistry.InstalledProperty, 1);

        var exeFile = setupData.GameType == GameType.Eaw
            ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;


        var fs = serviceProvider.GetRequiredService<IFileSystem>();
        var exePath = fs.Path.Combine(setupData.InstallPath!, exeFile);
        versionedKey.SetValue(GameRegistry.ExePathProperty, exePath);

        var installPath = customDirectoryInfo?.FullName ?? setupData.InstallPath!;
        versionedKey.SetValue(GameRegistry.InstallPathProperty, installPath);
    }
}