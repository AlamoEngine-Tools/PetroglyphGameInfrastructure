// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO.Abstractions;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Defines methods and properties for querying and manipulating a test Steam client's registry state.
/// </summary>
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

    /// <summary>
    /// Registers a test installation of the Steam client to the registry.
    /// </summary>
    [SupportedOSPlatform("windows")]
    void InstallSteam();

    /// <summary>
    /// Writes the specified process identifier as running Steam process to the registry.
    /// </summary>
    /// <param name="pid">The process identifier to set. <see langword="null"/> to indicate Steam is not running.</param>
    [SupportedOSPlatform("windows")]
    void SetPid(int? pid);

    /// <summary>
    /// Writes the identifier for the current user to the registry.
    /// </summary>
    /// <param name="userId">The unique identifier to assign to the user.</param>
    void SetUserId(uint userId);
}