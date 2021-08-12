using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamRegistry : IDisposable
    {
        int? ActiveUserId { get; internal set; }

        int? ProcessId { get; }

        IFileInfo? ExeFile { get; }

        IDirectoryInfo? InstallationDirectory { get; }

        ISet<string>? InstalledApps { get; }
    }
}