using System;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Games.Registry;

/// <summary>
/// Represents a wrapper for Registry values a Petroglyph Star Wars game uses.
/// </summary>
public interface IGameRegistry : IDisposable
{
    /// <summary>
    /// Gets the <see cref="GameType"/> this registry is dedicated to.
    /// </summary>
    GameType Type { get; }

    /// <summary>
    /// Gets a values indicating whether the underlying registry key exists.
    /// </summary>
    bool Exits { get; }

    /// <summary>
    /// Gets the version registry node.
    /// </summary>
    Version? Version { get; }

    /// <summary>
    /// Gets the installed license key.
    /// </summary>
    string? CdKey { get; }

    /// <summary>
    /// Unknown value.
    /// </summary>
    int? EaWGold { get; }

    /// <summary>
    /// Gets the game's exe file.
    /// </summary>
    IFileInfo? ExePath { get; }

    /// <summary>
    /// Gets the registry value, whether the game is installed.
    /// </summary>
    /// <remarks>
    /// This value should not be trusted as it is not ensured this value is set correctly by any client platform.
    /// </remarks>
    bool? Installed { get; }

    /// <summary>
    /// Directory where the game's install location. 
    /// </summary>
    /// <remarks>This does not necessarily point the same location as <see cref="ExePath"/>.</remarks>
    IDirectoryInfo? InstallPath { get; }

    /// <summary>
    /// Gets the file info of Petroglyph's native game launcher.
    /// </summary>
    IFileInfo? Launcher { get; }

    /// <summary>
    /// Gets the revision version of the game.
    /// </summary>
    int? Revision { get; }
}