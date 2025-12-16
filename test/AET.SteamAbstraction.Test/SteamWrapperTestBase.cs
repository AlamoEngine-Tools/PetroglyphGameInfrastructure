using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Testing;
using AET.SteamAbstraction.Utilities;
using AET.Testing.Extensions;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public abstract class SteamWrapperTestBase : IDisposable
{
    private readonly ISteamWrapperFactory _wrapperFactory;

    protected readonly MockFileSystem FileSystem = new();
    protected readonly IServiceProvider ServiceProvider;
    
    protected ITestingSteamInstallation? Steam;
    internal TestProcessHelper? ProcessHelper;

    protected SteamWrapperTestBase()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(FileSystem);
        SteamAbstractionLayer.InitializeServices(sc);
        sc.AddSingleton<TestProcessHelper>(sp =>
        {
            return ProcessHelper = new TestProcessHelper(sp);
        });
        sc.AddSingleton<IProcessHelper>(sp => sp.GetRequiredService<TestProcessHelper>());
        // ReSharper disable once VirtualMemberCallInConstructor
        BuildServiceCollection(sc);
        ServiceProvider = sc.BuildServiceProvider();
        _wrapperFactory = ServiceProvider.GetRequiredService<ISteamWrapperFactory>();
    }

    public void Dispose()
    {
        ProcessHelper?.KillCurrent();
    }

    protected virtual void BuildServiceCollection(IServiceCollection serviceCollection)
    {
    }

    [MemberNotNull(nameof(Steam))]
    protected void InstallSteam()
    {
        Steam = FileSystem.Steam(ServiceProvider);
        Steam.Install();
    }

    private protected SteamWrapper CreateWrapper()
    {
        return (SteamWrapper)_wrapperFactory.CreateWrapper();
    }

    [Fact]
    public async Task TestInstalled()
    {
        // Not installed at the moment
        using var wrapper = CreateWrapper();
        
        Assert.False(wrapper.Installed);
        Assert.False(wrapper.IsRunning);
        Assert.False(wrapper.IsUserLoggedIn);
        Assert.Null(wrapper.UserWantsOfflineMode);
        Assert.Empty(wrapper.Libraries);

        Assert.Throws<SteamNotFoundException>(() => wrapper.IsGameInstalled(123, out _));
        Assert.Throws<SteamNotFoundException>(wrapper.StartSteam);
        await Assert.ThrowsAsync<SteamNotFoundException>(() => wrapper.WaitSteamRunningAndLoggedInAsync(Random.Bool(), CancellationToken.None));

        // Install Steam
        InstallSteam();

        // Check Steam is installed but still not running, etc.
        Assert.True(wrapper.Installed); 
        Assert.False(wrapper.IsRunning);
        Assert.False(wrapper.IsUserLoggedIn);
        Assert.Null(wrapper.UserWantsOfflineMode);
        Assert.Empty(wrapper.Libraries);
    }

    [Fact]
    public async Task TestDisposed()
    {
        InstallSteam();

        var wrapper = CreateWrapper();
        wrapper.Dispose();

        Assert.Throws<ObjectDisposedException>(() => wrapper.Installed);
        Assert.Throws<ObjectDisposedException>(() => wrapper.IsRunning);
        Assert.Throws<ObjectDisposedException>(() => wrapper.IsUserLoggedIn);
        Assert.Throws<ObjectDisposedException>(() => wrapper.Libraries);
        Assert.Throws<ObjectDisposedException>(() => wrapper.UserWantsOfflineMode);
        Assert.Throws<ObjectDisposedException>(wrapper.StartSteam);
        Assert.Throws<ObjectDisposedException>(() => wrapper.IsGameInstalled(123, out _));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => wrapper.WaitSteamRunningAndLoggedInAsync(Random.Bool(), CancellationToken.None));
    }

    [Fact]
    public void IsRunning()
    {
        InstallSteam();

        var wrapper = CreateWrapper();

        FakeStartSteam();

        Assert.True(wrapper.IsRunning);
        Assert.False(wrapper.IsUserLoggedIn);
    }

    [Fact]
    public void IsRunning_RegistryHasPidSetButProcessNotRunning()
    {
        InstallSteam();

        var wrapper = CreateWrapper();

        var expectedPid = new Random().Next(0, int.MaxValue);
        Steam.Registry.SetPid(expectedPid);

        Assert.False(wrapper.IsRunning);
        Assert.False(wrapper.IsUserLoggedIn);
    }

    [Fact]
    public void Libraries()
    {
        InstallSteam();

        var wrapper = CreateWrapper();

        Assert.Empty(wrapper.Libraries);

        var expectedLib = Steam.InstallDefaultLibrary();
        var lib = Assert.Single(wrapper.Libraries);
        Assert.Equal(expectedLib, lib);

        var otherExpectedLib = Steam.InstallLibrary("externalLib");
        Assert.Equal(
            new List<ISteamLibrary>{ expectedLib, otherExpectedLib }.OrderBy(x => x.LibraryLocation.FullName),
            wrapper.Libraries.OrderBy(x => x.LibraryLocation.FullName));
    }

    [Fact]
    public void IsGameInstalled()
    {
        InstallSteam();

        var wrapper = CreateWrapper();

        // No Libs => no games
        Assert.False(wrapper.IsGameInstalled(123, out var game));
        Assert.Null(game);


        var lib = Steam.InstallDefaultLibrary();
        // One Lib with no games
        Assert.False(wrapper.IsGameInstalled(123, out game));
        Assert.Null(game);

        var expectedGame = lib.InstallGame(123, "MyGame");
        Assert.True(wrapper.IsGameInstalled(123, out game));
        Assert.NotNull(game);
        Assert.Equal(expectedGame.Id, game.Id);
        Assert.Equal(expectedGame.Name, game.Name);

        Assert.False(wrapper.IsGameInstalled(456, out var otherGame));
        Assert.Null(otherGame);


        var otherLib = Steam.InstallLibrary("externalLib");
        otherLib.InstallCorruptApp();
        var otherExpectedGame = otherLib.InstallGame(456, "OtherGame");
        Assert.True(wrapper.IsGameInstalled(456, out otherGame));
        Assert.NotNull(otherGame);
        Assert.Equal(otherExpectedGame.Id, otherGame.Id);
        Assert.Equal(otherExpectedGame.Name, otherGame.Name);
    }

    [Fact]
    public void IsUserLoggedIn()
    {
        InstallSteam();

        var wrapper = CreateWrapper();
        Assert.False(wrapper.IsUserLoggedIn);

        FakeStartSteam();
        Assert.False(wrapper.IsUserLoggedIn);

        LoginUser(123456789);
        Assert.True(wrapper.IsUserLoggedIn);

        LogoutUser();
        Assert.False(wrapper.IsUserLoggedIn);

        LoginUser(123456789);
        StopSteam();
        Assert.False(wrapper.IsUserLoggedIn);
    }

    [Fact]
    public void UserWantsOfflineMode()
    {
        InstallSteam();

        var wrapper = CreateWrapper();
        Steam.DeleteLoginUsersFile();
        Assert.Null(wrapper.UserWantsOfflineMode);

        Steam.WriteLoginUsers(new TestingSteamUserLoginMetadata(ulong.MaxValue, false, false));
        Assert.False(wrapper.UserWantsOfflineMode);

        // Not most recent
        Steam.WriteLoginUsers(new TestingSteamUserLoginMetadata(ulong.MaxValue, false, true));
        Assert.False(wrapper.UserWantsOfflineMode);

        Steam.WriteLoginUsers(new TestingSteamUserLoginMetadata(ulong.MaxValue, true, true));
        Assert.True(wrapper.UserWantsOfflineMode);

        Steam.WriteLoginUsers(
            new TestingSteamUserLoginMetadata(ulong.MaxValue, false, true),
            new TestingSteamUserLoginMetadata(456, true, true));
        Assert.True(wrapper.UserWantsOfflineMode);

        // Not most recent
        Steam.WriteLoginUsers(
            new TestingSteamUserLoginMetadata(ulong.MaxValue, false, true),
            new TestingSteamUserLoginMetadata(456, true, false));
        Assert.False(wrapper.UserWantsOfflineMode);

        Steam.WriteCorruptLoginUsers();
        Assert.Null(wrapper.UserWantsOfflineMode);
    }

    [Fact]
    public void StartSteam()
    {
        InstallSteam();

        var wrapper = CreateWrapper();

        wrapper.StartSteam();

        var currentProcess = ProcessHelper!.CurrentProcess;
        Assert.NotNull(currentProcess);
        Assert.True(wrapper.IsRunning);

        // Start again should not create a new process
        wrapper.StartSteam();
        Assert.Same(currentProcess, ProcessHelper.CurrentProcess);
        Assert.Equal(currentProcess.Id, ProcessHelper.CurrentProcess!.Id);

        ProcessHelper.KillCurrent();
        Assert.False(wrapper.IsRunning);
    }


    [Fact]
    public async Task WaitSteamRunningAndLoggedInAsync_SteamNotRunningAndShouldNotStart_ThrowsSteamException()
    {
        InstallSteam();
        var wrapper = CreateWrapper();

        LoginUser(12345);

        await Assert.ThrowsAsync<SteamException>(async () => await wrapper.WaitSteamRunningAndLoggedInAsync(false));

        Assert.Equal(0u, wrapper.GetCurrentUserId());
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_LoggedIn()
    {
        InstallSteam();
        var wrapper = CreateWrapper();

        FakeStartSteam();
        LoginUser(12345);

        await wrapper.WaitSteamRunningAndLoggedInAsync(false);

        Assert.Equal(12345u, wrapper.GetCurrentUserId());
    }

    [Fact]
    public async Task TestWaitSteamRunningAndLoggedInAsync_DelayedLogin()
    {
        InstallSteam();
        var wrapper = CreateWrapper();

        var waitTask = wrapper.WaitSteamRunningAndLoggedInAsync(true);

        // Ensure that code had time to proceed to past running check. then wait a little more.
        await wrapper.WaitSteamRunningAsync(CancellationToken.None)
            .ContinueWith(async _ => await Task.Delay(2000));

        Assert.True(wrapper.IsRunning);
        Assert.False(waitTask.IsCompleted);
        
        LoginUser(12345);

        await waitTask;
        
        Assert.Equal(12345u, wrapper.GetCurrentUserId());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WaitSteamRunningAndLoggedInAsync_SteamGetsStarted(bool delayStart)
    {
        InstallSteam();
        ServiceProvider.GetRequiredService<IProcessHelper>();
        ProcessHelper!.DelayStart = delayStart;

        var wrapper = CreateWrapper();

        var cts = new CancellationTokenSource();

        var steamStartedTask = Task.Run(async () => await wrapper.WaitSteamRunningAsync(CancellationToken.None), 
            CancellationToken.None);

        wrapper.WaitSteamRunningAndLoggedInAsync(true, cts.Token).Forget();

        await steamStartedTask;

        Assert.True(wrapper.IsRunning);

        // Ignore the rest
        cts.Cancel();
    }

    [Fact]
    public async Task WaitSteamRunningAndLoggedInAsync_UserDoesNotLoginBeforeCancellation_Throws()
    {
        InstallSteam();

        var wrapper = CreateWrapper();
        FakeStartSteam();

        var cts = new CancellationTokenSource();

        var task = wrapper.WaitSteamRunningAndLoggedInAsync(false, cts.Token);
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

    private void StopSteam()
    {
        Steam?.Registry.SetPid(null);
        ProcessHelper!.SetRunningPid(null);
    }

    private void FakeStartSteam()
    {
        Assert.NotNull(Steam);
        Steam.FakeStart(new Random().Next(100, 10000));
    }

    protected virtual void LogoutUser()
    {
        Assert.NotNull(Steam);
        Steam.Registry.SetUserId(0);
        Steam.WriteLoginUsers(null);
    }

    protected virtual void LoginUser(uint userId)
    {
        Assert.NotNull(Steam);
        Steam.Registry.SetUserId(userId);
        Steam.WriteLoginUsers(new TestingSteamUserLoginMetadata(userId, true, false));
    }
}