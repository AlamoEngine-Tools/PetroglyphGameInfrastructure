using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Sklavenwalker.CommonUtilities.Registry;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public interface ISteamRegistry : IDisposable
{
    IRegistryKey? ActiveProcessKey { get; }

    int? ActiveUserId { get; set; }

    int? ProcessId { get; }

    IFileInfo? ExeFile { get; }

    IDirectoryInfo? InstallationDirectory { get; }

    ISet<uint>? InstalledApps { get; }
}