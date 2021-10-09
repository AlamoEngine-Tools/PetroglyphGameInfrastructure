using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Registry;
using Validation;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// Windows only registry wrapper for an Petroglyph Star Wars game.
    /// </summary>
    public sealed class GameRegistry : IGameRegistry
    {
        private IRegistryKey? _registryKey;

        private const string VersionKey = "1.0";
        private const string CDKeyProperty = "CD Key";
        private const string EawGoldProperty = "EAWGold";
        private const string ExePathProperty = "ExePath";
        private const string InstalledProperty = "Installed";
        private const string InstallPathProperty = "InstallPath";
        private const string LauncherProperty = "Launcher";
        private const string RevisionProperty = "Revision";

        private static readonly Version VersionInstance = new(1, 0);

        private readonly IFileSystem _fileSystem;

        private bool _disposed;

        /// <inheritdoc/>
        public GameType Type { get; }

        /// <inheritdoc/>
        public bool Exits
        {
            get
            {
                ThrowIfDisposed();
                return _registryKey.GetKey(string.Empty) is not null;
            }
        }

        /// <inheritdoc/>
        public IGame? Game { get; private set; }

        /// <inheritdoc/>
        public Version? Version
        {
            get
            {
                ThrowIfDisposed();
                // Currently the there only exists a 1.x release of the game
                // and likely never to happen that we see a change here.
                // Thus we leave this part hardcoded and pray PG does not alter the deal.
                return _registryKey.HasPath(VersionKey) ? VersionInstance : null;
            }
        }

        /// <inheritdoc/>
        public string? CdKey
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(CDKeyProperty, VersionKey, out string? value, null))
                    return null;
                return value;
            }
        }

        /// <inheritdoc/>
        public int? EaWGold
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(EawGoldProperty, VersionKey, out int? value, null))
                    return null;
                return value;
            }
        }

        /// <inheritdoc/>
        public IFileInfo? ExePath
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(ExePathProperty, VersionKey, out string? value, null))
                    return null;
                if (string.IsNullOrEmpty(value))
                    return null;
                return _fileSystem.FileInfo.FromFileName(value);
            }
        }

        /// <inheritdoc/>
        public bool? Installed
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(InstalledProperty, VersionKey, out bool? value, null))
                    return null;
                return value;
            }
        }

        /// <inheritdoc/>
        public IDirectoryInfo? InstallPath
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(InstallPathProperty, VersionKey, out string? value, null))
                    return null;
                if (string.IsNullOrEmpty(value))
                    return null;
                return _fileSystem.DirectoryInfo.FromDirectoryName(value);
            }
        }

        /// <inheritdoc/>
        public IFileInfo? Launcher
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(LauncherProperty, VersionKey, out string? value, null))
                    return null;
                if (string.IsNullOrEmpty(value))
                    return null;
                return _fileSystem.FileInfo.FromFileName(value);
            }
        }

        /// <inheritdoc/>
        public int? Revision
        {
            get
            {
                ThrowIfDisposed();
                if (!_registryKey.GetValueOrDefault(RevisionProperty, VersionKey, out int? value, null))
                    return null;
                return value;
            }
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="gameType">The <see cref="GameType"/> this registry is associated to.</param>
        /// <param name="registryKey">The root key of the game's registry.</param>
        /// <param name="serviceProvider">Service provider for this instance.</param>
        public GameRegistry(GameType gameType, IRegistryKey registryKey, IServiceProvider serviceProvider)
        {
            Requires.NotNull(registryKey, nameof(registryKey));
            _registryKey = registryKey;
            _fileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
        }

        /// <inheritdoc/>
        ~GameRegistry()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void AssignGame(IGame? game)
        {
            Game = game;
        }

        /// <summary>
        /// Disposed all managed resources acquired by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposed all managed resources acquired by this instance.
        /// </summary>
        /// <param name="disposing"><see langword="false"/> if called from the destructor; <see langword="true"/> otherwise.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _registryKey?.Dispose();
                _registryKey = null;
                _disposed = true;
            }
        }
        

#if NET
        [MemberNotNull(nameof(_registryKey))]
#endif
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());
            if (_registryKey is null)
                throw new Exception("registry must not be null in non-disposed state");
        }
    }
}