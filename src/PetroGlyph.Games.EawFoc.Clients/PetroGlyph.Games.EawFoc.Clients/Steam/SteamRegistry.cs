using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Win32;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public sealed class SteamRegistry : ISteamRegistry
    {
        private const string SteamExeKey = "SteamExe";
        private const string SteamPathKey = "SteamPath";
        private const string SteamProcessIdKey = "pid";
        private const string SteamActiveUserKey = "ActiveUser";

        private const string SteamProcessNode = "ActiveProcess";

        private WindowsRegistryWrapper? _registry;
        private bool _disposed;
        private readonly IFileSystem _fileSystem;

        int? ISteamRegistry.ActiveUserId
        {
            get
            {
                ThrowIfDisposed();
                return !_registry.GetValue(SteamActiveUserKey, SteamProcessNode, out int? userId) ? null : userId;
            }
            set
            {
                ThrowIfDisposed();
                value ??= 0;
                _registry.WriteValue(SteamActiveUserKey, SteamProcessNode, value);
            }
        }

        public int? ProcessId
        {
            get
            {
                ThrowIfDisposed();
                return !_registry.GetValue(SteamProcessIdKey, SteamProcessNode, out int? pid) ? null : pid;
            }
        }

        public IFileInfo? ExeFile
        {
            get
            {
                ThrowIfDisposed();
                return !_registry.GetValue(SteamExeKey, out string? filePath)
                    ? null
                    : _fileSystem.FileInfo.FromFileName(filePath);
            }
        }

        public IDirectoryInfo? InstallationDirectory
        {
            get
            {
                ThrowIfDisposed();
                return !_registry.GetValue(SteamPathKey, out string? path)
                    ? null
                    : _fileSystem.DirectoryInfo.FromDirectoryName(path);
            }
        }

        public ISet<string>? InstalledApps { get; }


        public SteamRegistry(IFileSystem? fileSystem = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This instance is only available on Windows systems.");
            _registry = new WindowsRegistryWrapper(
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default), "Software/Valve/Steam");
            _fileSystem = fileSystem ?? new FileSystem();
        }


        /// <inheritdoc/>
        ~SteamRegistry()
        {
            Dispose(false);
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