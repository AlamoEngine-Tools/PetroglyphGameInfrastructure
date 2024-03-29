using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<ISteamRegistry> _steamRegistry;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<ISteamGameFinder> _gameFinder;

    public SteamWrapperTest()
    {
        var sc = new ServiceCollection();
        _steamRegistry = new Mock<ISteamRegistry>();
        _fileSystem = new MockFileSystem();
        _gameFinder = new Mock<ISteamGameFinder>();
        sc.AddTransient(_ => _steamRegistry.Object);
        sc.AddTransient(_ => new Mock<IProcessHelper>().Object);
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
    public void TestGameInstalled()
    {
        foreach (var steamWrapper in GetSteamWrappers()) 
            Assert.Throws<SteamNotFoundException>(() => steamWrapper.IsGameInstalled(0, out _));

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
        _fileSystem.Initialize()
            .WithFile("steam.exe")
            .WithSubdirectory("config");

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

    [Fact]
    public void TestUserLoggedIn()
    {
        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);

        wrapper.Setup(s => s.IsRunning).Returns(false);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns((int?)null);
        Assert.False(wrapper.Object.IsUserLoggedIn);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns(0);
        Assert.False(wrapper.Object.IsUserLoggedIn);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns(456);
        Assert.False(wrapper.Object.IsUserLoggedIn);

        wrapper.Setup(s => s.IsRunning).Returns(true);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns((int?)null);
        Assert.False(wrapper.Object.IsUserLoggedIn);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns(0);
        Assert.False(wrapper.Object.IsUserLoggedIn);

        _steamRegistry.Setup(r => r.ActiveUserId).Returns(456);
        Assert.True(wrapper.Object.IsUserLoggedIn);
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_SteamNotRunning_ThrowsSteamException()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
        _steamRegistry.Setup(r => r.ActiveUserId).Returns(123);

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);

        await Assert.ThrowsAsync<SteamException>(async() => await wrapper.Object.WaitSteamRunningAndLoggedInAsync(false));
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_LoggedIn()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
        _steamRegistry.Setup(r => r.ActiveUserId).Returns(123);
        
        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);
        wrapper.Setup(s => s.IsRunning).Returns(true);

        await wrapper.Object.WaitSteamRunningAndLoggedInAsync(false);
    }


    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_SteamNotRunning_StartSteam()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));
        _steamRegistry.Setup(r => r.ActiveUserId).Returns(123);

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);

        // We cannot start the process, so we just catch the exception that the process was attempted to start.
        await Assert.ThrowsAsync<Win32Exception>(async () => await wrapper.Object.WaitSteamRunningAndLoggedInAsync(true));
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_Login()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);
        wrapper.Setup(s => s.IsRunning).Returns(true);

        wrapper.Protected().Setup<Task>("WaitSteamUserLoggedInAsync", CancellationToken.None)
            .Callback((CancellationToken _) =>
            {
                _steamRegistry.Setup(r => r.ActiveUserId).Returns(456);
            })
            .Returns(Task.Delay(500));

        var task = wrapper.Object.WaitSteamRunningAndLoggedInAsync(false);
        Assert.False(task.IsCompleted);

        var completedTask = await Task.WhenAny(task, Task.Delay(5000));
        Assert.Same(task, completedTask);
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_NoLogin()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);
        wrapper.Setup(s => s.IsRunning).Returns(true);

        wrapper.Protected().Setup<Task>("WaitSteamUserLoggedInAsync", CancellationToken.None)
            .Returns(Task.Delay(5000));

        var task =  wrapper.Object.WaitSteamRunningAndLoggedInAsync(false);
        Assert.False(task.IsCompleted);

        var completedTask = await Task.WhenAny(task, Task.Delay(500));
        Assert.NotSame(task, completedTask);
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_WithOfflineMode()
    {
        _fileSystem.Initialize()
            .WithFile("steam.exe")
            .WithFile("config/loginusers.vdf").Which(a => a.HasStringContent(WantsOffline()));

        _steamRegistry.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.New("."));
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);
        wrapper.Setup(s => s.IsRunning).Returns(true);

        var tsc = new TaskCompletionSource<int>();

        wrapper.Protected().Setup<Task>("WaitSteamOfflineRunning", CancellationToken.None)
            .Returns(tsc.Task);

        var task = wrapper.Object.WaitSteamRunningAndLoggedInAsync(false);
        Assert.False(task.IsCompleted);

        _ = Task.Run(async () => await Task.Delay(1000).ContinueWith(_ => tsc.SetResult(1)));

        await task;
        Assert.True(tsc.Task.IsCompleted);
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_Cancelled()
    {
        _fileSystem.Initialize().WithFile("steam.exe");
        _steamRegistry.Setup(r => r.ExecutableFile).Returns(_fileSystem.FileInfo.New("steam.exe"));

        var wrapper = new Mock<SteamWrapper>(_steamRegistry.Object, _serviceProvider);
        wrapper.Setup(s => s.IsRunning).Returns(true);

        var cts = new CancellationTokenSource();

        wrapper.Protected().Setup<Task>("WaitSteamUserLoggedInAsync", cts.Token)
            .Returns(Task.Delay(5000, cts.Token));

        
        var task = wrapper.Object.WaitSteamRunningAndLoggedInAsync(false, cts.Token);
        Assert.False(task.IsCompleted);

        cts.Cancel();

        try
        {
            await task;
            Assert.Fail("Expected exception not thrown.");
        }
        catch (OperationCanceledException ex)
        {
            Assert.Equal(cts.Token, ex.CancellationToken);
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