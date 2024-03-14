using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Registry representation for the Steam Client.
/// </summary>
public interface ISteamRegistry : IDisposable
{
    /// <summary>
    /// Key to the ActiveProcess node
    /// </summary>
    IRegistryKey? ActiveProcessKey { get; }

    /// <summary>
    /// Id of the logged-in user.
    /// </summary>
    int? ActiveUserId { get; set; }

    /// <summary>
    /// PID of the Steam process.
    /// </summary>
    int? ProcessId { get; }

    /// <summary>
    /// The executable of the Steam client.
    /// </summary>
    IFileInfo? ExeFile { get; }

    /// <summary>
    /// The installation directory of the Steam client
    /// </summary>
    IDirectoryInfo? InstallationDirectory { get; }

    /// <summary>
    /// Set of installed app IDs.
    /// </summary>
    ISet<uint>? InstalledApps { get; }
}