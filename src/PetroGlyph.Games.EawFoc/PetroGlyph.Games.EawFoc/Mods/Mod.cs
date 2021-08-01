using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.FileService;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    /// <summary>
    /// An ordinary, physical mod.
    /// </summary>
    public class Mod : ModBase, IPhysicalMod
    {
        private IPhysicalFileService? _fileService;

        internal string InternalPath { get; }

        /// <inheritdoc/>
        public IDirectoryInfo Directory { get; }

        /// <inheritdoc/>
        public IFileSystem FileSystem => Directory.FileSystem;

        /// <inheritdoc/>
        public virtual IPhysicalFileService FileService
        {
            get
            {
                if (_fileService is not null)
                    return _fileService;
                var fs = ServiceProvider.GetService<IPhysicalFileServiceTest>() ?? (IPhysicalFileService?)new DefaultFileService(this);
                _fileService = fs!;
                return _fileService;
            }
        }

        /// <summary>
        /// The <see cref="IModinfoFile"/> which was set by an constructor, null <see langword="null"/> otherwise;
        /// </summary>
        public IModinfoFile? ModinfoFile { get; }

        /// <summary>
        /// Is this mod is a workshops mod, it holds the workshop ID, otherwise a normalized, absolute path
        /// </summary>
        public override string Identifier
        {
            get
            {
                return Type switch
                {
                    ModType.Default => InternalPath,
                    ModType.Workshops => Directory.Name,
                    ModType.Virtual => throw new ModException($"Instance of {GetType()} must not be virtual."),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="game">The game of the mod.</param>
        /// <param name="modDirectory">The mod's directory.</param>
        /// <param name="workshop">When set to <see langword="true"/> this instance is a Steam Workshop mod.</param>
        /// <param name="modinfoFile">The <see cref="IModinfoFile"/> which holds the mod's <see cref="IModinfo"/> data.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfoFile modinfoFile, IServiceProvider serviceProvider)
            : base(game, workshop ? ModType.Workshops : ModType.Default, modinfoFile?.GetModinfo()!, serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ModinfoFile = modinfoFile;
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="game">The game of the mod.</param>
        /// <param name="modDirectory">The mod's directory.</param>
        /// <param name="workshop">When set to <see langword="true"/> this instance is a Steam Workshop mod.</param>
        /// <param name="name">The name of the mod.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider)
            : base(game, workshop ? ModType.Workshops : ModType.Default, name, serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="game">The game of the mod.</param>
        /// <param name="modDirectory">The mod's directory.</param>
        /// <param name="workshop">When set to <see langword="true"/> this instance is a Steam Workshop mod.</param>
        /// <param name="modInfoData">The <see cref="IModinfo"/>.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData, serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            if (!modDirectory.Exists)
                throw new ModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        internal static string CreateInternalPath(IDirectoryInfo directory)
        {
            return directory.FileSystem.Path.NormalizePath(directory.FullName);
        }
    }
}