using System;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Registry;

/// <summary>
/// Registry representation for the Steam Client.
/// </summary>
internal interface ISteamRegistry : IDisposable
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
}