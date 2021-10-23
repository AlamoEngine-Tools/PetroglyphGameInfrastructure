using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

public class SteamAppManifest : IEquatable<SteamAppManifest>
{ 
    public ISteamLibrary Library { get; }

    public IFileInfo ManifestFile { get; }

    public uint Id { get; }

    public string Name { get; }

    public IDirectoryInfo InstallDir { get; }

    public SteamAppState State { get; }

    public ISet<uint> Depots { get; }

    public SteamAppManifest(
        ISteamLibrary library,
        IFileInfo manifestFile, 
        uint id, 
        string name, 
        IDirectoryInfo installDir,
        SteamAppState state, 
        ISet<uint> depots)
    {
        Requires.NotNull(library, nameof(library));
        Requires.NotNull(manifestFile, nameof(manifestFile));
        Requires.NotNullOrEmpty(name, nameof(name));
        Requires.NotNull(installDir, nameof(installDir));
        Requires.NotNull(depots, nameof(depots));
        Library = library;
        ManifestFile = manifestFile;
        Id = id;
        Name = name;
        InstallDir = installDir;
        State = state;
        Depots = depots;
    }

    public bool Equals(SteamAppManifest? other)
    {
        if (ReferenceEquals(this, other)) 
            return true;
        return Id == other?.Id && Library.Equals(other.Library);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) 
            return true;
        return obj is SteamAppManifest other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Id;
    }
}