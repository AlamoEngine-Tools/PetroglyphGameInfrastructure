using System;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// A Registry wrapper for an <see cref="IGame"/>.
    /// </summary>
    public interface IGameRegistry : IDisposable
    {
        /// <summary>
        /// The <see cref="GameType"/> this registry is dedicated to.
        /// </summary>
        GameType Type { get; }

        /// <summary>
        /// Key indication the game exists.
        /// </summary>
        bool Exits { get; }

        /// <summary>
        /// The game this instance is associated too.
        /// </summary>
        IGame? Game { get; }

        /// <summary>
        /// The version registry node.
        /// </summary>
        Version? Version { get; }

        /// <summary>
        /// The installed license key.
        /// </summary>
        string? CdKey { get; }

        /// <summary>
        /// Unknown value.
        /// </summary>
        int? EaWGold { get; }

        /// <summary>
        /// The game's exe file.
        /// </summary>
        IFileInfo? ExePath { get; }

        /// <summary>
        /// Useless indication whether the game is installed,
        /// because there exists no native mechanism which sets this property to the correct value.
        /// </summary>
        bool? Installed { get; }

        /// <summary>
        /// Directory where the game's install location. 
        /// </summary>
        /// <remarks>This does not necessarily point the same location as <see cref="ExePath"/>.</remarks>
        IDirectoryInfo? InstallPath { get; }

        /// <summary>
        /// The Petroglyph's native game launcher.
        /// </summary>
        IFileInfo? Launcher { get; }

        /// <summary>
        /// The revision version of the game.
        /// </summary>
        int? Revision { get; }

        /// <summary>
        /// Sets the <see cref="Game"/> property.
        /// </summary>
        /// <param name="game">The game instance or <see langword="null"/> to unassign the game from this instance.</param>
        void AssignGame(IGame? game);
    }
}