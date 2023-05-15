using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    internal sealed class SteamRegistry : ISteamRegistry
    {
        private const string SteamExeKey = "SteamExe";
        private const string SteamPathKey = "SteamPath";
        private const string SteamProcessIdKey = "pid";
        private const string SteamActiveUserKey = "ActiveUser";

        private const string SteamProcessNode = "ActiveProcess";
        private const string SteamAppsNode = "Apps";

        private IRegistryKey? _registryKey;
        private bool _disposed;
        private readonly IFileSystem _fileSystem;

        public IRegistryKey? ActiveProcessKey
        {
            get
            {
                ThrowIfDisposed();
                return _registryKey!.GetKey(SteamProcessNode);
            }
        }

        int? ISteamRegistry.ActiveUserId
        {
            get
            {
                ThrowIfDisposed();
                return !_registryKey!.GetValue(SteamActiveUserKey, SteamProcessNode, out int? userId) ? null : userId;
            }
            set
            {
                ThrowIfDisposed();
                value ??= 0;
                _registryKey!.WriteValue(SteamActiveUserKey, SteamProcessNode, value);
            }
        }

        public int? ProcessId
        {
            get
            {
                ThrowIfDisposed();
                return !_registryKey!.GetValue(SteamProcessIdKey, SteamProcessNode, out int? pid) ? null : pid;
            }
        }

        public IFileInfo? ExeFile
        {
            get
            {
                ThrowIfDisposed();
                return !_registryKey!.GetValue(SteamExeKey, out string? filePath)
                    ? null
                    : _fileSystem.FileInfo.New(filePath!);
            }
        }

        public IDirectoryInfo? InstallationDirectory
        {
            get
            {
                ThrowIfDisposed();
                return !_registryKey!.GetValue(SteamPathKey, out string? path)
                    ? null
                    : _fileSystem.DirectoryInfo.New(path!);
            }
        }

        public ISet<uint>? InstalledApps
        {
            get
            {
                ThrowIfDisposed();
                var keyNames = _registryKey!.GetSubKeyNames(SteamAppsNode);
                if (keyNames is null)
                    return null;
                var ids = keyNames
                    .Select(n => !uint.TryParse(n, out var id) ? (uint?)0 : id)
                    .OfType<uint>();
                return new HashSet<uint>(ids);
            }
        }


        public SteamRegistry(IServiceProvider serviceProvider)
        {
            _registryKey = serviceProvider.GetRequiredService<IRegistry>()
                .OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                .CreateSubKey("Software\\Valve\\Steam");
            _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
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