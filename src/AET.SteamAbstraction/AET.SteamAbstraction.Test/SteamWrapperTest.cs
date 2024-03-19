using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<ISteamRegistry> _steamRegistry;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IProcessHelper> _processHelper;
    private readonly Mock<ISteamGameFinder> _gameFinder;

    public SteamWrapperTest()
    {
        var sc = new ServiceCollection();
        _steamRegistry = new Mock<ISteamRegistry>();
        _fileSystem = new MockFileSystem();
        _processHelper = new Mock<IProcessHelper>();
        _gameFinder = new Mock<ISteamGameFinder>();
        sc.AddTransient(_ => _steamRegistry.Object);
        sc.AddTransient(_ => _processHelper.Object);
        sc.AddTransient(_ => _gameFinder.Object);
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _serviceProvider = sc.BuildServiceProvider();
    }

    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(null!, _serviceProvider));
        Assert.Throws<ArgumentNullException>(() => new WindowsSteamWrapper(_steamRegistry.Object, null!));
        Assert.Throws<ArgumentNullException>(() => new LinuxSteamWrapper(null!, _serviceProvider));
        Assert.Throws<ArgumentNullException>(() => new LinuxSteamWrapper(_steamRegistry.Object, null!));
    }

    [Fact]
    public void TestRunning()
    {
        foreach (var steamWrapper in GetSteamWrappers())
        {
            _processHelper.SetupSequence(h => h.GetProcessByPid(It.IsAny<int>()))
                .Returns((Process)null)
                .Returns(Process.GetCurrentProcess);
            _steamRegistry.SetupSequence(r => r.ProcessId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123)
                .Returns(123);

            Assert.False(steamWrapper.IsRunning);
            Assert.False(steamWrapper.IsRunning);
            Assert.False(steamWrapper.IsRunning);
            Assert.True(steamWrapper.IsRunning);
        }
    }

    [Fact]
    public void TestInstalled()
    {
        _steamRegistry.Setup(r => r.ExecutableFile).Returns((IFileInfo?)null);

        foreach (var steamWrapper in GetSteamWrappers())
        {
            _steamRegistry.Setup(r => r.ExecutableFile).Returns((IFileInfo?)null);
            Assert.False(steamWrapper.Installed);

            _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
            Assert.False(steamWrapper.Installed);
        }
        
        _fileSystem.Initialize().WithFile("steam.exe");

        foreach (var steamWrapper in GetSteamWrappers())
        {
            _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
            Assert.True(steamWrapper.Installed);
        }
    }

    [Fact]
    public void TestDisposed()
    {
        var counter = 0;
        foreach (var steamWrapper in GetSteamWrappers())
        {
            steamWrapper.Dispose();
            _steamRegistry.Verify(r => r.Dispose(), Times.Exactly(++counter));
        }
    }


    [Fact]
    public void TestUserLoggedIn()
    {
        foreach (var steamWrapper in GetSteamWrappers())
        {
            _steamRegistry.SetupSequence(r => r.ActiveUserId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123);

            Assert.False(steamWrapper.IsUserLoggedIn);
            Assert.False(steamWrapper.IsUserLoggedIn);
            Assert.True(steamWrapper.IsUserLoggedIn);
        }
    }

    [Fact]
    public void TestGameInstalled()
    {
        foreach (var steamWrapper in GetSteamWrappers()) 
            Assert.Throws<SteamNotFoundException>(() => steamWrapper.IsGameInstalled(0, out _));

        _fileSystem.Initialize();
        _fileSystem.Initialize().WithFile("steam.exe");

        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
        _steamRegistry.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.New("."));


        var mFile = _fileSystem.FileInfo.New("manifest.acf");

        var expectedApp = new SteamAppManifest(new Mock<ISteamLibrary>().Object, mFile, 1234, "name",
            _fileSystem.DirectoryInfo.New("Game"), SteamAppState.StateFullyInstalled,
            new HashSet<uint>());

        _steamRegistry.Setup(r => r.InstalledApps).Returns(new HashSet<uint> { 1, 2, 3 });
        _steamRegistry.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.New("Steam"));

        foreach (var steamWrapper in GetSteamWrappers())
        {
            _gameFinder.SetupSequence(f => f.FindGame(It.IsAny<uint>()))
                .Returns((SteamAppManifest?)null)
                .Returns(expectedApp);

            Assert.False(steamWrapper.IsGameInstalled(0, out _));
            Assert.False(steamWrapper.IsGameInstalled(1, out _));
            Assert.True(steamWrapper.IsGameInstalled(1, out var app));

            Assert.Same(expectedApp, app);
        }
    }

    [Fact]
    public void TestStartStream_Throws()
    {
        foreach (var steamWrapper in GetSteamWrappers())
            Assert.Throws<SteamNotFoundException>(() => steamWrapper.StartSteam());
    }

    [Fact]
    public void TestWantsOffline()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _fileSystem.Initialize().WithSubdirectory("config");

        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
        _steamRegistry.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.New("."));

        foreach (var steamWrapper in GetSteamWrappers()) 
            Assert.Null((object?)steamWrapper.UserWantsOfflineMode);

        foreach (var steamWrapper in GetSteamWrappers())
        {
            _fileSystem.File.Create("config/loginusers.vdf").Dispose();
            Assert.Null((object?)steamWrapper.UserWantsOfflineMode);

            _fileSystem.File.WriteAllText("config/loginusers.vdf", WantsNotOffline());
            Assert.False(steamWrapper.UserWantsOfflineMode);

            _fileSystem.File.WriteAllText("config/loginusers.vdf", WantsOffline());
            Assert.True(steamWrapper.UserWantsOfflineMode);
        }
    }

    public IEnumerable<ISteamWrapper> GetSteamWrappers()
    {
        yield return new WindowsSteamWrapper(_steamRegistry.Object, _serviceProvider);
        yield return new LinuxSteamWrapper(_steamRegistry.Object, _serviceProvider);
    }

    private static string WantsNotOffline()
    {
        return "\"users\"\n{\n\t\"123\"\n\t{\n\t\t\"AccountName\"\t\t\"user_name\"\n\t\t\"PersonaName\"\t\t\"User Name\"\n\t\t\"RememberPassword\"\t\t\"1\"\n\t\t\"MostRecent\"\t\t\"1\"\n\t\t\"Timestamp\"\t\t\"0000000000\"\n\t\t\"WantsOfflineMode\"\t\t\"0\"\n\t\t\"SkipOfflineModeWarning\"\t\t\"0\"\n\t}\n}";
    }

    private static string WantsOffline()
    {
        return "\"users\"\n{\n\t\"123\"\n\t{\n\t\t\"AccountName\"\t\t\"user_name\"\n\t\t\"PersonaName\"\t\t\"User Name\"\n\t\t\"RememberPassword\"\t\t\"1\"\n\t\t\"MostRecent\"\t\t\"1\"\n\t\t\"Timestamp\"\t\t\"0000000000\"\n\t\t\"WantsOfflineMode\"\t\t\"1\"\n\t\t\"SkipOfflineModeWarning\"\t\t\"0\"\n\t}\n}";
    }
}