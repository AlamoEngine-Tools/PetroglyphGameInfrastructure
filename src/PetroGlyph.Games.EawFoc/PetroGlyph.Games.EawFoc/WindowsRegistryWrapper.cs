using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Validation;

namespace PetroGlyph.Games.EawFoc
{
    /// <summary>
    /// Windows registry wrapper instance, which operates on a <see cref="RootKey"/> and a fixed BasePath
    /// </summary>
    public sealed class WindowsRegistryWrapper : IWindowsRegistry
    {
        /// <summary>
        /// The base path used for this instance.
        /// </summary>
        public string BasePath { get; }

        /// <inheritdoc/>
        public RegistryKey RootKey { get; }

        /// <summary>
        /// Creates a new wrapper instance.
        /// </summary>
        /// <param name="baseKey">The registry base key, containing the requested <see cref="RegistryHive"/> and <see cref="RegistryView"/></param>
        /// <param name="basePath">The base path</param>
        /// <exception cref="PlatformNotSupportedException">When created from a non-Windows system.</exception>
        public WindowsRegistryWrapper(RegistryKey baseKey, string? basePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This instance is only supported on windows systems");
            Requires.NotNull(baseKey, nameof(baseKey));
            RootKey = baseKey;
            BasePath = basePath ?? string.Empty;
        }

#pragma warning disable CA1416 // Check Platform compatibility
        
        /// <inheritdoc/>
        public bool GetValueOrDefault<T>(string name, string subPath, out T? result, T? defaultValue)
        {
            result = defaultValue;
            using var key = GetKey(subPath);
            var value = key?.GetValue(name, defaultValue);
            if (value is null)
                return false;

            try
            {
                result = (T)value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool GetValueOrDefault<T>(string name, out T? result, T? defaultValue)
        {
            return GetValueOrDefault(name, string.Empty, out result, defaultValue);
        }

        /// <inheritdoc/>
        public bool HasPath(string path)
        {
            using var key = GetKey(path);
            return key != null;
        }

        /// <inheritdoc/>
        public bool HasValue(string name)
        {
            return GetValue<object>(name, out _);
        }

        /// <inheritdoc/>
        public bool GetValue<T>(string name, string subPath, out T? value)
        {
            return GetValueOrDefault(name, subPath, out value, default);
        }

        /// <inheritdoc/>
        public bool GetValue<T>(string name, out T? value)
        {
            return GetValue(name, string.Empty, out value);
        }

        /// <inheritdoc/>
        public RegistryKey? GetKey(string subPath, bool writable = false)
        {
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
            return RootKey.OpenSubKey(Path.Combine(BasePath, subPath), writable);
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability
        }

        /// <inheritdoc/>
        public T? GetValueOrSetDefault<T>(string name, T? defaultValue, out bool defaultValueUsed)
        {
            return GetValueOrSetDefault(name, string.Empty, defaultValue, out defaultValueUsed);
        }

        /// <inheritdoc/>
        public T? GetValueOrSetDefault<T>(string name, string subPath, T? defaultValue, out bool defaultValueUsed)
        {
            defaultValueUsed = !GetValue<T>(name, subPath, out var value);
            if (defaultValueUsed)
            {
                if (defaultValue is null) 
                    return value;
                WriteValue(name, subPath, defaultValue);
                return defaultValue;

            }
            return value;
        }

        /// <inheritdoc/>
        public bool WriteValue(string name, object value)
        {
            return WriteValue(name, string.Empty, value);
        }

        /// <inheritdoc/>
        public bool WriteValue(string name, string subPath, object value)
        {
            try
            {
                using var key = GetKey(subPath, true);
                if (key is null)
                    return false;
                key.SetValue(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool DeleteValue(string name)
        {
            return DeleteValue(name, string.Empty);
        }

        /// <inheritdoc/>
        public bool DeleteValue(string name, string subPath)
        {
            try
            {
                using var key = GetKey(subPath, true);
                if (key is null)
                    return false;
                key.DeleteValue(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public string[]? GetSubKeyNames(string subPath)
        {
            using var key = GetKey(subPath);
            if (key is null)
                return null;
            try
            {
                return key.GetSubKeyNames();
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RootKey.Dispose();
        }
#pragma warning restore CA1416 // Check Platform compatibility
    }
}