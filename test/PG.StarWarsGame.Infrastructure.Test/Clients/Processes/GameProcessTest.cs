using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;
using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Processes;

public class GameProcessLauncherTest : CommonTestBaseWithRandomGame, IDisposable
{
    private readonly GameProcessLauncher _launcher;
    private readonly IFileInfo _executable;

    // We need to use the real FS here, cause Process.Start uses it too.
    private readonly FileSystem _realFileSystem = new();

    private GameProcess? _gameProcess;

    public GameProcessLauncherTest()
    {
        _launcher = new GameProcessLauncher(ServiceProvider);

        var tempDir = _realFileSystem.Path.Combine(_realFileSystem.Path.GetTempPath(), "GameProcessLauncherTest");
        _realFileSystem.Directory.CreateDirectory(tempDir);

        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "TestExecutable.bat" : "TestExecutable.sh";
        _executable = _realFileSystem.FileInfo.New(_realFileSystem.Path.Combine(tempDir, executableName));

        var scriptContent = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "@echo off\nexit 0"
            : "#!/bin/bash\nexit 0";

        _realFileSystem.File.WriteAllText(_executable.FullName,scriptContent);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            Process.Start("chmod", $"+x {_executable.FullName}").WaitForExit();
    }

    [Fact]
    public void StartGameProcess_ValidExecutable_Succeeds()
    {
        var gameProcessInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);

        var process = _gameProcess = (GameProcess)_launcher.StartGameProcess(_executable, gameProcessInfo);

        Assert.NotNull(process);
        Assert.Equal(GameProcessState.Running, process.State);

        var internalProcess = process.Process;

        Assert.Same(gameProcessInfo, process.ProcessInfo);
        Assert.Equal(_executable.Directory.FullName, internalProcess.StartInfo.WorkingDirectory);
        Assert.Equal(_executable.FullName, internalProcess.StartInfo.FileName);
        Assert.Empty(internalProcess.StartInfo.Arguments);
        Assert.False(internalProcess.StartInfo.UseShellExecute);

        process.Exit();
        process.Process.WaitForExit();
        Assert.Equal(GameProcessState.Closed, process.State);
    }

    [Fact]
    public void StartGameProcess_VerifyProcessArgumentsAndUseShellExecute()
    {
        var arguments = new ArgumentCollection([new WindowedArgument()]);
        var gameProcessInfo = new GameProcessInfo(Game, GameBuildType.Release, arguments);

        var process = _gameProcess = (GameProcess)_launcher.StartGameProcess(_executable, gameProcessInfo);

        Assert.NotNull(process);
        Assert.Equal(GameProcessState.Running, process.State);

        var internalProcess = process.Process;

        Assert.Same(gameProcessInfo, process.ProcessInfo);
        Assert.Equal(_executable.Directory.FullName, internalProcess.StartInfo.WorkingDirectory);
        Assert.Equal(_executable.FullName, internalProcess.StartInfo.FileName);
        Assert.Contains("WINDOWED", internalProcess.StartInfo.Arguments);
        Assert.False(internalProcess.StartInfo.UseShellExecute);

        process.Exit();
        process.Process.WaitForExit();
        Assert.Equal(GameProcessState.Closed, process.State);
    }

    [Fact]
    public async Task StartGameProcess_ValidExecutable_WaitForExitAsync()
    {
        var gameProcessInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);

        var process = _gameProcess = (GameProcess)_launcher.StartGameProcess(_executable, gameProcessInfo);

        Assert.NotNull(process);
        Assert.Equal(GameProcessState.Running, process.State);

        var internalProcess = process.Process;

        Assert.Same(gameProcessInfo, process.ProcessInfo);
        Assert.Equal(_executable.Directory.FullName, internalProcess.StartInfo.WorkingDirectory);
        Assert.Equal(_executable.FullName, internalProcess.StartInfo.FileName);
        Assert.Empty(internalProcess.StartInfo.Arguments);

        await process.WaitForExitAsync();
        Assert.Equal(GameProcessState.Closed, process.State);
    }

    [Fact]
    public void StartGameProcess_InvalidExecutable_ThrowsGameStartException()
    {
        var invalidExecutable = FileSystem.FileInfo.New("invalid/path/to/executable");
        var gameProcessInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);

        Assert.Throws<GameStartException>(() => _launcher.StartGameProcess(invalidExecutable, gameProcessInfo));
    }

    [Fact]
    public void StartGameProcess_InvalidArguments_ThrowsGameStartException()
    {
        var gameProcessInfo = new GameProcessInfo(Game, GameBuildType.Release, new ArgumentCollection([new CDKeyArgument("invalid arg")]));

        Assert.Throws<GameStartException>(() => _launcher.StartGameProcess(_executable, gameProcessInfo));
    }

    public void Dispose()
    {
        try
        {
            _gameProcess?.Process.Kill();
            _gameProcess?.Dispose();

            var tempDir = _realFileSystem.Path.GetDirectoryName(_executable.FullName);
            if (_realFileSystem.Directory.Exists(tempDir))
                _realFileSystem.Directory.Delete(tempDir, true);
        }
        catch
        {
        }
    }
}

public class GameProcessTest : CommonTestBaseWithRandomGame, IDisposable
{
    private Process _testProcess = null!;

    private Process StartStableTestProcess()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
            Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c pause" : "-c read",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
        };
        _testProcess = Process.Start(processStartInfo)!;
        return _testProcess;
    }

    public void Dispose()
    {
        try
        {
            _testProcess?.Kill();
            _testProcess?.Dispose();
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    [Fact]
    public async Task WaitForGameProcessExitAsync_CompletesOnProcessExit()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        Assert.Equal(GameProcessState.Running, gameProcess.State);

        var exitTask = gameProcess.WaitForExitAsync();
        process.StandardInput.WriteLine();

        await exitTask;
        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public async Task WaitForGameProcessExitAsync_CancelsOnTokenCancellation()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);
        var cts = new CancellationTokenSource();

        var waitTask = gameProcess.WaitForExitAsync(cts.Token);
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => waitTask);
        Assert.Equal(GameProcessState.Running, gameProcess.State);
    }

    [Fact]
    public async Task Exit_ClosesProcess()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);
        Assert.Equal(GameProcessState.Running, gameProcess.State);

        gameProcess.Exit();
        
        process.WaitForExit();

        await gameProcess.WaitForExitAsync();

        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);

        // Double kill should not throw
        gameProcess.Exit();
    }

    [Fact]
    public async Task Exit_DoubleDoesNotThrow()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);
        Assert.Equal(GameProcessState.Running, gameProcess.State);

        var b = new Barrier(2);

        var t1 = Task.Run(() =>
        {
            b.SignalAndWait();
            gameProcess.Exit();
        });
        var t2 = Task.Run(() =>
        {
            b.SignalAndWait();
            gameProcess.Exit();
        });


        await Task.WhenAll(t1, t2);

        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void ProcessInfo_ReturnsExpectedInfo()
    {
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var process = StartStableTestProcess();
        var gameProcess = new GameProcess(process, processInfo);

        Assert.Equal(processInfo, gameProcess.ProcessInfo);
    }

    [Fact]
    public void State_ReturnsExpectedState()
    {
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var process = StartStableTestProcess();
        var gameProcess = new GameProcess(process, processInfo);

        Assert.Equal(GameProcessState.Running, gameProcess.State);
        process.StandardInput.WriteLine();

        process.WaitForExit();
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void Constructor_AlreadyClosedProcess_DoesNotThrow()
    {
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var process = StartStableTestProcess();
        process.Kill();
        process.WaitForExit();

        var gameProcess = new GameProcess(process, processInfo);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void Constructor_AlreadyDisposedProcess_Throws()
    {
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var process = StartStableTestProcess();
        process.Dispose();

        Assert.Throws<InvalidOperationException>(() => new GameProcess(process, processInfo));
    }

    [Fact]
    public void Constructor_ThrowsForNotStartedProcess()
    {
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        var notStartedProcess = new Process { StartInfo = processStartInfo };

        Assert.Throws<InvalidOperationException>(() => new GameProcess(notStartedProcess, processInfo));
    }

    [Fact]
    public async Task ClosedEvent_IsRaisedWhenProcessExits()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        var eventRaiseCount = 0;
        gameProcess.Closed += (_, _) => eventRaiseCount++;

        process.StandardInput.WriteLine();
        process.WaitForExit();

        Assert.Equal(1, eventRaiseCount);
        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void ClosedEvent_IsRaisedWhenExitCalled()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        var eventRaised = false;
        gameProcess.Closed += (_, _) => eventRaised = true;

        gameProcess.Exit();
        process.WaitForExit();

        Assert.True(eventRaised);
        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void ClosedEvent_IsRaisedImmediatelyForAlreadyExitedProcess()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        process.Kill(); // Force process to exit immediately
        process.WaitForExit();

        var gameProcess = new GameProcess(process, processInfo);

        var eventRaised = false;
        gameProcess.Closed += (_, _) => eventRaised = true;

        Assert.True(eventRaised);
        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void ClosedEvent_HandlerRemoved_IsNotCalled()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        var eventRaised = false;
        EventHandler handler = (_, _) => eventRaised = true;

        // Attach and then remove the handler
        gameProcess.Closed += handler;
        gameProcess.Closed -= handler;

        gameProcess.Exit();
        process.WaitForExit();

        Assert.False(eventRaised);
        Assert.True(process.HasExited);
        Assert.Equal(GameProcessState.Closed, gameProcess.State);
    }

    [Fact]
    public void Dispose_UnregistersClosedEventHandlers()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        bool eventRaised = false;
        EventHandler handler = (_, _) => eventRaised = true;

        gameProcess.Closed += handler;
        gameProcess.Dispose();
        gameProcess.Closed -= handler; // This should not throw but also not do anything

        Assert.False(eventRaised);
        // Should not throw
        gameProcess.Closed += handler;
    }

    [Fact]
    public void Dispose_ClosesProcessAndCleansResources()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        gameProcess.Dispose();

        Assert.Throws<InvalidOperationException>(gameProcess.Exit);
        // Should not throw
        gameProcess.Closed += (_, _) => { };

        Assert.Throws<InvalidOperationException>(() => process.HasExited);
    }

    [Fact]
    public async Task Dispose_CompletesWaitForExitAsyncSilently()
    {
        var process = StartStableTestProcess();
        var processInfo = new GameProcessInfo(Game, GameBuildType.Release, ArgumentCollection.Empty);
        var gameProcess = new GameProcess(process, processInfo);

        var waitTask = gameProcess.WaitForExitAsync();

        gameProcess.Dispose();

        // WaitForExitAsync continues listening
        var t = await Task.WhenAny(waitTask, Task.Delay(2000));
        Assert.NotSame(t, waitTask);
    }
}