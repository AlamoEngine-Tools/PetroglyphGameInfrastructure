using System;
using Microsoft.Win32;

namespace PetroGlyph.Games.EawFoc
{
    /// <summary>
    /// High-Level abstraction layer for the Windows Registry.
    /// Each method is scoped to a the instance's <see cref="RootKey"/>.
    /// This service supports read and write operations.
    /// </summary>
    public interface IWindowsRegistry : IDisposable
    {
        /// <summary>
        /// The root key of this instance.
        /// All methods use this key as a base and subpath are built based on this key.
        /// It also holds the registry view (32bit/64bit).
        /// </summary>
        RegistryKey RootKey { get; }

        /// <summary>
        /// Tries to get a value from a registry key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub path of the key.</param>
        /// <param name="result">The returned value or <paramref name="defaultValue"/> if no value could be found.</param>
        /// <param name="defaultValue"></param>
        bool GetValueOrDefault<T>(string name, string subPath, out T? result, T? defaultValue);

        /// <summary>
        /// Tries to get a value from a registry key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="result">The returned value or <paramref name="defaultValue"/> if no value could be found.</param>
        /// <param name="defaultValue"><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        bool GetValueOrDefault<T>(string name, out T? result, T? defaultValue);

        /// <summary>
        /// Checks whether a given path exists in the registry.
        /// </summary>
        /// <param name="path">The requested path.</param>
        /// <returns><see langword="true"/> if a path exists; <see langword="false"/> otherwise.</returns>
        bool HasPath(string path);

        /// <summary>
        /// Checks whether a given key exists in the registry.
        /// </summary>
        /// <param name="name">The requested key.</param>
        /// <returns><see langword="true"/> if a key exists; <see langword="false"/> otherwise.</returns>
        bool HasValue(string name);

        /// <summary>
        /// Gets the value of a given key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub path of the key.</param>
        /// <param name="value">The returned value or <typeparamref name="T"/> default value if no value could be found.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        bool GetValue<T>(string name, string subPath, out T? value);

        /// <summary>
        /// Gets the value of a given key.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="value">The returned value or <typeparamref name="T"/> default value if no value could be found.</param>
        /// <returns><see langword="true"/> if a value was found; <see langword="false"/> otherwise.</returns>
        bool GetValue<T>(string name, out T? value);

        /// <summary>
        /// Gets the <see cref="RegistryKey"/> of a given path.
        /// </summary>
        /// <param name="subPath">The sub-path.</param>
        /// <param name="writable">Set to <see langword="true"/> if write access is required. Default is <see langword="false"/>.</param>
        /// <returns>The registry key or <see langword="null"/> if the operation failed.</returns>
        RegistryKey? GetKey(string subPath, bool writable = false);

        /// <summary>
        ///  Tries to get a value from a registry key. If the key did not exist, a key with a given default value gets created.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="defaultValue">The default value which shall be set</param>
        /// <param name="defaultValueUsed">Gets set to <see langword="true"/> if <paramref name="defaultValue"/> was set; <see langword="false"/> otherwise.</param>
        /// <returns>The actual or default value of the key.</returns>
        T? GetValueOrSetDefault<T>(string name, T? defaultValue, out bool defaultValueUsed);

        /// <summary>
        ///  Tries to get a value from a registry key. If the key did not exist, a key with a given default value gets created.
        /// </summary>
        /// <typeparam name="T">The requested type of the key's value.</typeparam>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub-path.</param>
        /// <param name="defaultValue">The default value which shall be set.</param>
        /// <param name="defaultValueUsed">Gets set to <see langword="true"/> if <paramref name="defaultValue"/> was set; <see langword="false"/> otherwise.</param>
        /// <returns>The actual or default value of the key.</returns>
        /// <exception cref="InvalidOperationException">If the requested registry key could not be found.</exception>
        T? GetValueOrSetDefault<T>(string name, string subPath, T? defaultValue, out bool defaultValueUsed);

        /// <summary>
        ///  Tries to write a value from a registry key.
        /// </summary>
        /// <param name="name">The name of the key.</param>
        /// <param name="value">The value which shall be set.</param>
        /// <returns><see langword="true"/> if a value was set successfully; <see langword="false"/> otherwise.</returns>
        bool WriteValue(string name, object value);

        /// <summary>
        ///  Tries to write a value from a registry key.
        /// </summary>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub-path.</param>
        /// <param name="value">The value which shall be set.</param>
        /// <returns><see langword="true"/> if a value was set successfully; <see langword="false"/> otherwise.</returns>
        bool WriteValue(string name, string subPath, object value);

        /// <summary>
        ///  Tries to delete a value from a registry key.
        /// </summary>
        /// <param name="name">The name of the key.</param>
        /// <returns><see langword="true"/> if a value was deleted successfully; <see langword="false"/> otherwise.</returns>
        bool DeleteValue(string name);

        /// <summary>
        ///  Tries to delete a value from a registry key.
        /// </summary>
        /// <param name="name">The name of the key.</param>
        /// <param name="subPath">The sub-path.</param>
        /// <returns><see langword="true"/> if a value was deleted successfully; <see langword="false"/> otherwise.</returns>
        bool DeleteValue(string name, string subPath);
    }
}