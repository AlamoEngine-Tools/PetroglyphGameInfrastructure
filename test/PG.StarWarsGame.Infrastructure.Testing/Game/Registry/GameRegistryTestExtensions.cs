using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Registry;

public static class GameRegistryTestExtensions
{
    public static IGameRegistry CreateNonExistingRegistry(this GameType gameType, IServiceProvider serviceProvider)
    {
        var factory = new GameRegistryFactory(serviceProvider);
        return factory.CreateRegistry(gameType);
    }
    
    public static IGameRegistry Create(this TestGameRegistrySetupData registrySetupData, IServiceProvider serviceProvider)
    {
        var gameRegistry = CreateNonExistingRegistry(registrySetupData.GameType, serviceProvider);
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        InitializeRegistry(registry, registrySetupData, null, serviceProvider);
        return gameRegistry;
    }

    private static void InitializeRegistry(
        IRegistry registry, 
        TestGameRegistrySetupData setupData, 
        IDirectoryInfo? customDirectoryInfo, IServiceProvider serviceProvider)
    {
        var gameKeyPath = setupData.GameType == GameType.Eaw ? GameRegistryFactory.EawRegistryPath : GameRegistryFactory.FocRegistryPath;

        using var hklm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
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