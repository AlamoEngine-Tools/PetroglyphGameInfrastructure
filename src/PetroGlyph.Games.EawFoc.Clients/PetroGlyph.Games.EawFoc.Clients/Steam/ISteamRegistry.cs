using System.Collections.Generic;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamRegistry
    {
        int? ActiveUserId { get; }

        int? ProcessId { get; }

        IFileInfo ExeFile { get; }

        IDirectoryInfo InstallationDirectory { get; }

        ISet<string>? InstalledApps { get; }
    }
}