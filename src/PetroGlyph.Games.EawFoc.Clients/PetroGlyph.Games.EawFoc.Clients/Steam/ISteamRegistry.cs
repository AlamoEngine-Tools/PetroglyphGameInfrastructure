using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Win32;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamRegistry : IDisposable
    {
        RegistryKey? ActiveProcessKey { get; }

        int? ActiveUserId { get; set; }

        int? ProcessId { get; }

        IFileInfo? ExeFile { get; }

        IDirectoryInfo? InstallationDirectory { get; }

        ISet<uint>? InstalledApps { get; }
    }
}