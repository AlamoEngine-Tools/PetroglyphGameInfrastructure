using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// Empire at War Windows Registry wrapper
    /// </summary>
    public sealed class EawWindowsRegistry : WindowsGameRegistry
    {
        private const string EawRegistryPath =
            @"SOFTWARE\LucasArts\Star Wars Empire at War";

        /// <inheritdoc/>
        public override GameType Type => GameType.EaW;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="fileSystem">Optional custom filesystem implementation.
        /// Uses the native file system implementation when <see langword="null"/> is passed.</param>
        public EawWindowsRegistry(IFileSystem? fileSystem = null) : base(EawRegistryPath, fileSystem)
        {
        }
    }
}
