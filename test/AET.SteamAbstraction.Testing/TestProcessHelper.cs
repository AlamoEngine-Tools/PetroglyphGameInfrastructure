using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Provides a test for managing and simulating process-related operations.
/// </summary>
public sealed class TestProcessHelper : IProcessHelper
{
    private readonly IFileSystem _fileSystem;
    private readonly ITestingSteamRegistry _registry;

    private int? _pid;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProcessHelper"/> class using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    public TestProcessHelper(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _registry = SteamTesting.SteamRegistry(serviceProvider);
    }


    /// <summary>
    /// Gets the actual process linked to this <see cref="IProcessHelper"/>.
    /// </summary>
    public Process? CurrentProcess { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the process start should be delayed when <see cref="StartProcess"/> is called.
    /// </summary>
    public bool DelayStart { get; set; }

    /// <summary>
    /// Sets the running process identifier.
    /// </summary>
    /// <remarks>
    /// This method can be used to simulate a running process with the specified PID without actually starting a real process using <see cref="StartProcess"/>.
    /// </remarks>
    /// <param name="pid">The PID to use. Use <see langword="null"/> to indicate no process is running.</param>
    public void SetRunningPid(int? pid)
    {
        _pid = pid;
    }

    /// <summary>
    /// Checks whether the specified process identifier is the same as the process identifier linked to this <see cref="IProcessHelper"/>.
    /// </summary>
    /// <remarks>
    /// The process identifier of this <see cref="IProcessHelper"/> is either set by using <see cref="SetRunningPid(int?)"/> or <see cref="StartProcess"/>.
    /// </remarks>
    /// <param name="pid"></param>
    /// <returns>
    /// <see langword="true"/> if a process with the specified PID is running; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsProcessRunning(int pid)
    {
        return pid == _pid;
    }

    /// <summary>
    /// Starts a new process using the specified <see cref="ProcessStartInfo"/> configuration.
    /// </summary>
    /// <param name="startInfo"> The <see cref="ProcessStartInfo"/> object that specifies the configuration for the process to be started.</param>
    /// <returns> A <see cref="Process"/> object that represents the started process, or <see langword="null"/> if the process could not be started.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no file name is specified in <paramref name="startInfo"/>.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown if an error occurs when opening the associated file.</exception>
    [SupportedOSPlatform("windows")]
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

    /// <summary>
    /// Terminates the current process linked to this <see cref="IProcessHelper"/>, if any and sets the PID to <see langword="null"/>.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public void KillCurrent()
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