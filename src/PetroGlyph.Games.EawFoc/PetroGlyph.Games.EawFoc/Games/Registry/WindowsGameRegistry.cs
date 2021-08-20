﻿using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Validation;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// Windows only registry wrapper for an Petroglyph Star Wars game.
    /// </summary>
    public abstract class WindowsGameRegistry : IGameRegistry
    {
        private WindowsRegistryWrapper? _registry;

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
        public abstract GameType Type { get; }

        /// <inheritdoc/>
        public bool Exits
        {
            get
            {
                ThrowIfDisposed();
                return _registry!.GetKey(string.Empty) is not null;
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
                return _registry!.HasPath(VersionKey) ? VersionInstance : null;
            }
        }

        /// <inheritdoc/>
        public string? CdKey
        {
            get
            {
                ThrowIfDisposed();
                if (!_registry!.GetValueOrDefault(CDKeyProperty, VersionKey, out string? value, null))
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
                if (!_registry!.GetValueOrDefault(EawGoldProperty, VersionKey, out int? value, null))
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
                if (!_registry!.GetValueOrDefault(ExePathProperty, VersionKey, out string? value, null))
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
                if (!_registry!.GetValueOrDefault(InstalledProperty, VersionKey, out bool? value, null))
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
                if (!_registry!.GetValueOrDefault(InstallPathProperty, VersionKey, out string? value, null))
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
                if (!_registry!.GetValueOrDefault(LauncherProperty, VersionKey, out string? value, null))
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
                if (!_registry!.GetValueOrDefault(RevisionProperty, VersionKey, out int? value, null))
                    return null;
                return value;
            }
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="basePath">Registry base path</param>
        /// <param name="fileSystem">Optional custom filesystem implementation.
        /// Uses the native file system implementation when <see langword="null"/> is passed.</param>
        protected WindowsGameRegistry(string basePath, IFileSystem? fileSystem = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException("This instance is only supported on windows systems");

            Requires.NotNullOrEmpty(basePath, nameof(basePath));
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            _registry = new WindowsRegistryWrapper(baseKey, basePath);
            _fileSystem = fileSystem ?? new FileSystem();
        }

        /// <inheritdoc/>
        ~WindowsGameRegistry()
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _registry?.Dispose();
                _registry = null;
                _disposed = true;
            }
        }
        

#if NET
        [MemberNotNull(nameof(_registry))]
#endif
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());
            if (_registry is null)
                throw new Exception("registry must not be null in non-disposed state");
        }
    }
}