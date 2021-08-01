using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Validation;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// Windows registry wrapper instance.
    /// </summary>
    public sealed class WindowsReadonlyRegistry : IDisposable
    {
        private readonly RegistryKey _rootKey;
        private readonly string _basePath;

        /// <summary>
        /// Creates a new wrapper instance.
        /// </summary>
        /// <param name="rootKey">The registry root key</param>
        /// <param name="basePath">The base path</param>
        public WindowsReadonlyRegistry(RegistryKey rootKey, string? basePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException("This instance is only supported on windows systems");
            Requires.NotNull(rootKey, nameof(rootKey));
            _rootKey = rootKey;
            _basePath = basePath ?? string.Empty;
        }

#pragma warning disable CA1416 // Check Platform compatibility

        /// <summary>
        /// Tries to get a value from a registry key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub path of the key.</param>
        /// <param name="result">The returned value or <paramref name="defaultValue"/> if no value could be found.</param>
        /// <param name="defaultValue"></param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
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

        /// <summary>
        /// Tries to get a value from a registry key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="result">The returned value or <paramref name="defaultValue"/> if no value could be found.</param>
        /// <param name="defaultValue"><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        public bool GetValueOrDefault<T>(string name, out T? result, T? defaultValue)
        {
            return GetValueOrDefault(name, string.Empty, out result, defaultValue);
        }

        /// <summary>
        /// Checks whether a given path exists in the registry.
        /// </summary>
        /// <param name="path">The requested path.</param>
        /// <returns><see langword="true"/> if a path exists; <see langword="false"/> otherwise.</returns>
        public bool HasPath(string path)
        {
            using var key = GetKey(path);
            return key != null;
        }

        /// <summary>
        /// Checks whether a given key exists in the registry.
        /// </summary>
        /// <param name="name">The requested key.</param>
        /// <returns><see langword="true"/> if a key exists; <see langword="false"/> otherwise.</returns>
        public bool HasValue(string name)
        {
            return GetValue<object>(name, out _);
        }

        /// <summary>
        /// Gets the value of a given key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub path of the key.</param>
        /// <param name="value">The returned value or <typeparamref name="T"/> default value if no value could be found.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        public bool GetValue<T>(string name, string subPath, out T? value)
        {
            return GetValueOrDefault(name, subPath, out value, default);
        }

        /// <summary>
        /// Gets the value of a given key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="value">The returned value or <typeparamref name="T"/> default value if no value could be found.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        public bool GetValue<T>(string name, out T? value)
        {
            return GetValue(name, string.Empty, out value);
        }

        /// <summary>
        /// Gets the <see cref="RegistryKey"/> of a given path.
        /// </summary>
        /// <param name="subPath">The sub-path</param>
        /// <param name="writable">Set to <see langword="true"/> if write access is required. Default is <see langword="false"/>.</param>
        /// <returns></returns>
        public RegistryKey? GetKey(string subPath, bool writable = false)
        {
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
            return _rootKey.OpenSubKey(Path.Combine(_basePath, subPath), writable);
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability
        }

        /// <summary>
        /// Disposed acquired registry resources.
        /// </summary>
        public void Dispose()
        {
            _rootKey.Dispose();
        }
#pragma warning restore CA1416 // Check Platform compatibility
    }
}