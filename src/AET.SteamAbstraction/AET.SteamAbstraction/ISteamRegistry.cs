using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;

namespace AET.SteamAbstraction;

/// <summary>
/// Registry representation for the Steam Client.
/// </summary>
internal interface ISteamRegistry : IDisposable
{
    /// <summary>
    /// Id of the logged-in user.
    /// </summary>
    int? ActiveUserId { get; set; }

    /// <summary>
    /// The executable of the Steam client.
    /// </summary>
    IFileInfo? ExecutableFile { get; }

    /// <summary>
    /// The installation directory of the Steam client
    /// </summary>
    IDirectoryInfo? InstallationDirectory { get; }
}

internal interface IWindowsSteamRegistry : ISteamRegistry
{
    IRegistryKey? ActiveProcessKey { get; }
    
    int? ProcessId { get; }
    
    ISet<uint>? InstalledApps { get; }
}