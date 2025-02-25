﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Library;
using AnakinRaW.CommonUtilities;

namespace AET.SteamAbstraction.Games;

/// <summary>
/// Manifest representing an installed Steam game.
/// <para>Equality matching is only performed on the <see cref="Id"/> and <see cref="Library"/> properties.</para>
/// </summary>
public sealed class SteamAppManifest : IEquatable<SteamAppManifest>
{ 
    /// <summary>
    /// Gets the library where this game is installed.
    /// </summary>
    public ISteamLibrary Library { get; }

    /// <summary>
    /// Gets the file of this manifest.
    /// </summary>
    public IFileInfo ManifestFile { get; }

    /// <summary>
    /// Gets the game's ID.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the game's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the game's install directory.
    /// </summary>
    public IDirectoryInfo InstallDir { get; }

    /// <summary>
    /// Gets the state of the game when this instance was created.
    /// </summary>
    public SteamAppState State { get; }

    /// <summary>
    /// Gets the depots installed with this game. 
    /// </summary>
    /// <remarks>Usually this includes DLCs, language packs and required shared libraries.</remarks>
    public ISet<uint> Depots { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamAppManifest"/> class with the specified game information.
    /// </summary>
    /// <param name="library">The library where this game is installed.</param>
    /// <param name="manifestFile">The file of this manifest.</param>
    /// <param name="id">The game's ID.</param>
    /// <param name="name">The game's name.</param>
    /// <param name="installDir">The game's install directory.</param>
    /// <param name="state">The state of the game when this instance was created.</param>
    /// <param name="depots">The depots installed with this game. </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="library"/> or <paramref name="manifestFile"/> of <paramref name="installDir"/> or
    /// <paramref name="depots"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public SteamAppManifest(
        ISteamLibrary library,
        IFileInfo manifestFile, 
        uint id, 
        string name, 
        IDirectoryInfo installDir,
        SteamAppState state, 
        ISet<uint> depots)
    {
        if (library == null)
            throw new ArgumentNullException(nameof(library));
        if (manifestFile == null) 
            throw new ArgumentNullException(nameof(manifestFile));
        if (installDir == null)
            throw new ArgumentNullException(nameof(installDir));
        if (depots == null) 
            throw new ArgumentNullException(nameof(depots));
        ThrowHelper.ThrowIfNullOrEmpty(name);

        Library = library;
        ManifestFile = manifestFile;
        Id = id;
        Name = name;
        InstallDir = installDir;
        State = state;
        Depots = depots;
    }

    /// <inheritdoc/>
    public bool Equals(SteamAppManifest? other)
    {
        if (ReferenceEquals(this, other)) 
            return true;
        return Id == other?.Id && Library.Equals(other.Library);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) 
            return true;
        return obj is SteamAppManifest other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Library);
    }
}