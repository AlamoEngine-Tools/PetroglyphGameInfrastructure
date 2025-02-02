using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

namespace PG.StarWarsGame.Infrastructure.Testing.Clients;

public class TestGameProcessLauncher : IGameProcessLauncher, IDisposable
{
    private Process? _process;

    public bool ThrowsGameStartException { get; set; }

    public IFileInfo ExpectedExecutable { get; set; } = null!;

    public GameProcessInfo ExpectedProcessInfo { get; set; } = null!;

    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        Assert.Equal(ExpectedExecutable.FullName, executable.FullName);
        Assert.Same(ExpectedProcessInfo.Game, processInfo.Game);
        Assert.Equal(ExpectedProcessInfo.BuildType, processInfo.BuildType);
        Assert.Equal(ExpectedProcessInfo.Arguments, processInfo.Arguments);

        if (ThrowsGameStartException)
            throw new GameStartException(processInfo.Game, "Some exception");

        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
        var process = new Process();
        process.StartInfo.FileName = processName;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        _process = process;
        return new GameProcess(process, processInfo);
    }

    public void Dispose()
    {
        try
        {
            _process?.Kill();
            _process?.Dispose();
        }
        catch
        {
            // Ignore
        }
    }
}