using System;
using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

public interface ITestingSteamRegistry : IDisposable
{
    /// <summary>
    /// Gets the executable of the Steam client.
    /// </summary>
    IFileInfo? ExecutableFile { get; }

    /// <summary>
    /// Gets the installation directory of the Steam client
    /// </summary>
    IDirectoryInfo? InstallationDirectory { get; }

    /// <summary>
    /// The PID of the current Steam Process. 0 or <see langword="null"/> if Steam is not running.
    /// </summary>
    int? ProcessId { get; }

    [SupportedOSPlatform("windows")]
    void InstallSteam();

    [SupportedOSPlatform("windows")]
    void SetPid(int? pid);

    void SetUserId(uint userId);
}