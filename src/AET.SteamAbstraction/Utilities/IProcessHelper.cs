using System;
using System.Diagnostics;

namespace AET.SteamAbstraction.Utilities;

/// <summary>
/// Provides utility methods for managing and interacting with system processes.
/// </summary>
internal interface IProcessHelper
{
    /// <summary>
    /// Determines whether a process with the specified process identifier (PID) is currently running.
    /// </summary>
    /// <param name="pid">The process identifier (PID) to check.</param>
    /// <returns>
    /// <see langword="true"/> if a process with the specified PID is running; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method attempts to retrieve the process by its PID. If the process does not exist or cannot be accessed,
    /// it returns <see langword="false"/>.
    /// </remarks>
    bool IsProcessRunning(int pid);

    /// <summary>
    /// Starts a new process using the specified <see cref="ProcessStartInfo"/> configuration.
    /// </summary>
    /// <param name="startInfo"> The <see cref="ProcessStartInfo"/> object that specifies the configuration for the process to be started.</param>
    /// <returns> A <see cref="Process"/> object that represents the started process, or <see langword="null"/> if the process could not be started.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no file name is specified in <paramref name="startInfo"/>.</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown if an error occurs when opening the associated file.</exception>
    Process? StartProcess(ProcessStartInfo startInfo);
}