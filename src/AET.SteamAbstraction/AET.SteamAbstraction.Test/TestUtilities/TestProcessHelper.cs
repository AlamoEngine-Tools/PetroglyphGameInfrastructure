using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test.TestUtilities;

internal class TestProcessHelper(IServiceProvider sp) : IProcessHelper
{
    private readonly ISteamRegistryFactory _registryFactory = sp.GetRequiredService<ISteamRegistryFactory>();
    private readonly IFileSystem _fileSystem = sp.GetRequiredService<IFileSystem>();

    private int? _pid;

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

    public Process? StartProcess(ProcessStartInfo startInfo)
    {
        var registry = _registryFactory.CreateRegistry();
        var expectedFileName = registry.ExecutableFile?.FullName;
        Assert.Equal(expectedFileName, _fileSystem.Path.GetFullPath(startInfo.FileName));

        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";

        if (DelayStart)
            Task.Delay(2000).Wait();

        var p = Process.Start(new ProcessStartInfo(processName) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden })!;
        var pid = p.Id;

        SetRunningPid(pid);
        registry.SetPid(pid);

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
            _registryFactory.CreateRegistry().SetPid(null);
        }
        catch
        {
            // Ignore
        }
    }
}