﻿using System;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Representation of a Petroglyph Star Wars Game.
/// </summary>
public interface IGame : IModContainer, IPhysicalPlayableObject, IGameIdentity, IEquatable<IGame>
{
    /// <summary>
    /// Checks whether this game instance exists on this machine
    /// </summary>
    /// <returns><see langword="true"/> when the game exists; <see langword="false"/> otherwise.</returns>
    bool Exists();

    /// <summary>
    /// <see cref="IDirectoryInfo"/> of the "Mods" directory.
    /// <remarks>The result does not point to the Workshops directory.</remarks>
    /// </summary>
    IDirectoryInfo ModsLocation { get; }
}