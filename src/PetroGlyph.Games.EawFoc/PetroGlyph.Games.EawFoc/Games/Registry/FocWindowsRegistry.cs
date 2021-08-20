using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    /// <summary>
    /// Forces of Corruption Windows Registry wrapper
    /// </summary>
    public class FocWindowsRegistry : WindowsGameRegistry
    {
        private const string FocRegistryPath =
            @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";

        /// <inheritdoc/>
        public override GameType Type => GameType.Foc;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="fileSystem">Optional custom filesystem implementation.
        /// Uses the native file system implementation when <see langword="null"/> is passed.</param>
        public FocWindowsRegistry(IFileSystem? fileSystem = null) : base(FocRegistryPath, fileSystem)
        {
        }
    }
}