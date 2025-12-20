using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Represents a testing Steam installation that can be manipulated for testing purposes.
/// </summary>
public interface ITestingSteamInstallation : IDisposable
{
    /// <summary>
    /// Gets the directory where the Steam is installed or <see langword="null"/> if the test installation is not installed.
    /// </summary>
    IDirectoryInfo? InstallationDirectory { get; }

    /// <summary>
    /// Gets the testing registry.
    /// </summary>
    ITestingSteamRegistry Registry { get; }

    /// <summary>
    /// Installs the testing Steam installation to the file system and registry.
    /// </summary>
    [SupportedOSPlatform("windows")]
    void Install();

    /// <summary>
    /// Installs the testing Steam installation to the file system only.
    /// </summary>
    void InstallSteamFilesOnly();

    /// <summary>
    /// Starts a simulated Steam process for the specified process ID.
    /// </summary>
    /// <param name="pid">The process identifier (PID) of the process to simulate.</param>
    /// <returns>An <see cref="ISteamFakeProcess"/> instance representing the simulated Steam process.</returns>
    [SupportedOSPlatform("windows")]
    ISteamFakeProcess FakeStart(int pid);

    /// <summary>
    /// Installs the default Steam game library.
    /// </summary>
    /// <param name="addToConfig"><see langword="true"/> to add the installed library to the configuration; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</param>
    /// <returns>An <see cref="ITestingSteamLibrary"/> instance representing the newly installed game library.</returns>
    ITestingSteamLibrary InstallDefaultLibrary(bool addToConfig = true);

    /// <summary>
    /// Installs a new Steam game library at the specified path and returns the library.
    /// </summary>
    /// <param name="path">The path where the new Steam library will be installed.</param>
    /// <param name="addToConfig"><see langword="true"/> to add the installed library to the configuration; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</param>
    /// <returns>An <see cref="ITestingSteamLibrary"/> instance representing the newly installed game library.</returns>
    ITestingSteamLibrary InstallLibrary(string path, bool addToConfig = true);

    /// <summary>
    /// Writes an invalid "loginusers.vdf".
    /// </summary>
    void WriteCorruptLoginUsers();

    /// <summary>
    /// Deletes the "loginusers.vdf" file.
    /// </summary>
    void DeleteLoginUsersFile();

    /// <summary>
    /// Writes the specified Steam user login information to a new "loginusers.vdf".
    /// </summary>
    /// <param name="users">A parameter array of collections containing the login metadata for each Steam user to write.</param>
    /// <returns>An <see cref="IFileInfo"/> representing the file containing the written login user data.</returns>
    IFileInfo WriteLoginUsers(params IEnumerable<TestingSteamUserLoginMetadata>? users);
}