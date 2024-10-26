using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Games.Registry;

public class GameRegistryTest
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;

    public GameRegistryTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton(_registry);
        _serviceProvider = sc.BuildServiceProvider();
    }

    private GameRegistry CreateRegistry(GameType type, IRegistryKey baseKey, string subPath)
    {
        return new GameRegistry(type, baseKey, subPath, _serviceProvider);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void Ctor_ThrowsForInvalidArgs(GameType gameType)
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        Assert.Throws<ArgumentNullException>(() => new GameRegistry(gameType, null!, "subPath", _serviceProvider));
        Assert.Throws<ArgumentNullException>(() => new GameRegistry(gameType, baseKey, null!, _serviceProvider));
        Assert.Throws<ArgumentNullException>(() => new GameRegistry(gameType, baseKey, "subPath", null!));
        Assert.Throws<ArgumentException>(() => new GameRegistry(gameType, baseKey, "", _serviceProvider));
    }


    [Theory]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void Exits_DoesOnlyExistsIfRegistrySubKeyExists(GameType gameType, string subPath)
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

        var registry = CreateRegistry(gameType, baseKey, subPath);
        
        // The subPath does not yet exist.
        Assert.False(registry.Exits);

        // Create the sub path
        baseKey.CreateSubKey(subPath);

        // The subPath now exist.
        Assert.True(registry.Exits);
    }


    [Theory]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void Dispose_GetPropertyWhenDisposedShallThrow(GameType gameType, string subPath)
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        
        var registry = CreateRegistry(gameType, baseKey, subPath);
        baseKey.CreateSubKey(subPath);

        registry.Dispose();
        // Double dispose should not throw
        registry.Dispose();

        Assert.Throws<ObjectDisposedException>(() => registry.Exits);
        Assert.Throws<ObjectDisposedException>(() => registry.Installed);
        Assert.Throws<ObjectDisposedException>(() => registry.CdKey);
        Assert.Throws<ObjectDisposedException>(() => registry.EaWGold);
        Assert.Throws<ObjectDisposedException>(() => registry.InstallPath);
        Assert.Throws<ObjectDisposedException>(() => registry.Launcher);
        Assert.Throws<ObjectDisposedException>(() => registry.Revision);
        Assert.Throws<ObjectDisposedException>(() => registry.Version);

        // Should not throw
        Assert.Equal(gameType, registry.Type);
    }

    [Theory]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void
        GetProperties_ReturnDefaultValuesIfSubKeyDoesNotExist_ThenAlsoReturnsDefaultValuesOnceSubKeyExistsWithNoValueKeys
        (
            GameType gameType,
            string subPath
        )
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var registry = CreateRegistry(gameType, baseKey, subPath);
        Assert.False(registry.Exits);

        Assert.Equal(gameType, registry.Type);
        AssertDefaultRegistry(registry, false);

        // Create the sub key
        baseKey.CreateSubKey(subPath);

        AssertDefaultRegistry(registry, true);
    }

    [Theory]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void
        GetProperties_ReturnDefaultValuesIfSubKeyExistsWithNoValueKeys_ThenReturnCorrectValuesOnceValueKeysExist
        (
            GameType gameType,
            string subPath
        )
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var registry = CreateRegistry(gameType, baseKey, subPath);

        var gameSubKey = baseKey.CreateSubKey(subPath);
        Assert.NotNull(gameSubKey);

        AssertDefaultRegistry(registry, true);

        SetupRegistry(gameSubKey);

        AssertRegistryValues(registry);
    }
    
    [Theory]
    [InlineData(GameType.Eaw, GameRegistryFactory.EawRegistryPath)]
    [InlineData(GameType.Foc, GameRegistryFactory.FocRegistryPath)]
    public void GetProperties_RequiresVersion_1_0(GameType gameType, string subPath)
    {
        var baseKey = _registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var registry = CreateRegistry(gameType, baseKey, subPath);
        var gameSubKey = baseKey.CreateSubKey(subPath);
        Assert.NotNull(gameSubKey);
        var versionSubKey = gameSubKey.CreateSubKey("2.3");
        Assert.NotNull(versionSubKey);

        // Creating a value to the "2.3" sub key will not get reflected to the game registry, as it requires "1.0" as sub key.
        versionSubKey.SetValue("Installed", 1);
        Assert.Null(registry.Installed);
    }


    private void AssertRegistryValues(GameRegistry registry)
    {
        Assert.Equal(new Version(1, 0), registry.Version);
        Assert.Equal("cdkey", registry.CdKey);
        Assert.Equal(12345, registry.EaWGold);
        Assert.Equal(_fileSystem.FileInfo.New("exePath").FullName, registry.ExePath!.FullName);
        Assert.True(registry.Installed);
        Assert.Equal(_fileSystem.FileInfo.New("installPath").FullName, registry.InstallPath!.FullName);
        Assert.Equal(_fileSystem.FileInfo.New("launcherPath").FullName, registry.Launcher!.FullName);
        Assert.Equal(9876, registry.Revision);
    }

    private static void SetupRegistry(IRegistryKey baseKey)
    {
        using var versionKey = baseKey.CreateSubKey("1.0");
        Assert.NotNull(versionKey);
        versionKey.SetValue("CD Key", "cdkey");
        versionKey.SetValue("EAWGold", 12345);
        versionKey.SetValue("ExePath", "exePath");
        versionKey.SetValue("Installed", 1);
        versionKey.SetValue("InstallPath", "installPath");
        versionKey.SetValue("Launcher", "launcherPath");
        versionKey.SetValue("Revision", 9876);
    }

    private static void AssertDefaultRegistry(IGameRegistry registry, bool subKeyExists)
    {
        Assert.Equal(subKeyExists, registry.Exits);
        Assert.Equal(default, registry.Installed);
        Assert.Equal(default, registry.CdKey);
        Assert.Equal(default, registry.EaWGold);
        Assert.Equal(default, registry.InstallPath);
        Assert.Equal(default, registry.Launcher);
        Assert.Equal(default, registry.Revision);
        Assert.Equal(default, registry.Version);
    }
}