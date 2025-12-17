using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace AET.SteamAbstraction.Testing;

public sealed class TestProcessHelper : IProcessHelper
{
    private readonly IFileSystem _fileSystem;
    private readonly ITestingSteamRegistry _registry;

    private int? _pid;

    public TestProcessHelper(IServiceProvider sp)
    {
        _fileSystem = sp.GetRequiredService<IFileSystem>();
        _registry = SteamTesting.SteamRegistry(sp);
    }

    public Process? CurrentProcess { get; private set; }

    public bool DelayStart { get; set; }

    public void SetRunningPid(int? pid)
    {
        _pid = pid;
    }

    public bool IsProcessRunning(int pid)
    {
        return pid == _pid;
    }

    public Process StartProcess(ProcessStartInfo startInfo)
    {
        var expectedFileName = _registry.ExecutableFile?.FullName;
        Assert.Equal(expectedFileName, _fileSystem.Path.GetFullPath(startInfo.FileName));

        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";

        if (DelayStart)
            Task.Delay(2000).Wait();

        var p = Process.Start(new ProcessStartInfo(processName) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden })!;
        var pid = p.Id;

        SetRunningPid(pid);
        _registry.SetPid(pid);

        CurrentProcess = p;
        return p;
    }

    internal void KillCurrent()
    {
        try
        {
            CurrentProcess?.Kill();
            CurrentProcess = null;
            SetRunningPid(null);
            _registry.SetPid(null);
        }
        catch
        {
            // Ignore
        }
    }
}