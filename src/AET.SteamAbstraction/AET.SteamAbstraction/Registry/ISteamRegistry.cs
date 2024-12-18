using System;
using System.IO.Abstractions;

namespace AET.SteamAbstraction.Registry;

/// <summary>
/// Registry representation for the Steam Client.
/// </summary>
internal interface ISteamRegistry : IDisposable
{
    /// <summary>
    /// Gets the ID of the logged-in user.
    /// </summary>
    int? ActiveUserId { get; set; }

    /// <summary>
    /// Gets the executable of the Steam client.
    /// </summary>
    IFileInfo? ExecutableFile { get; }

    /// <summary>
    /// Gets the installation directory of the Steam client
    /// </summary>
    IDirectoryInfo? InstallationDirectory { get; }
}