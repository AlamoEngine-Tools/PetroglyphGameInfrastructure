using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Processes;

public class GameProcessLauncherTest : CommonTestBaseWithRandomGame, IDisposable
{
    private readonly GameProcessLauncher _launcher;
    private readonly IFileInfo _executable;

    // We need to use the real FS here, cause Process.Start uses it too.
    private readonly IFileSystem _realFileSystem = new RealFileSystem();

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
            : "#!/bin/bash\nsleep 5\nexit 0";

        _realFileSystem.File.WriteAllText(_executable.FullName,scriptContent);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            Process.Start("chmod", $"+x {_executable.FullName}")!.WaitForExit();
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
        Assert.Equal(_executable.Directory!.FullName, internalProcess.StartInfo.WorkingDirectory);
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
        Assert.Equal(_executable.Directory!.FullName, internalProcess.StartInfo.WorkingDirectory);
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
        Assert.Equal(_executable.Directory!.FullName, internalProcess.StartInfo.WorkingDirectory);
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
            // Ignore
        }
    }
}