using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.FileService;
using Sklavenwalker.CommonUtilities.FileSystem;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// An ordinary, physical mod.
/// </summary>
public class Mod : ModBase, IPhysicalMod
{
    private IPhysicalFileService? _fileService;
    private string? _identifier;

    internal string InternalPath { get; }

    /// <summary>
    /// Service for handling file system paths.
    /// </summary>
    protected readonly IPathHelperService PathHelperService;

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
            var fs = ServiceProvider.GetService<IPhysicalFileServiceTest>() ??
                     (IPhysicalFileService?)new DefaultFileService(this);
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
            if (string.IsNullOrEmpty(_identifier))
            {
                var identifierBuilder = ServiceProvider.GetService<IModIdentifierBuilder>()
                                        ?? new ModIdentifierBuilder(ServiceProvider);
                _identifier = identifierBuilder.Build(this);
            }
            return _identifier!;
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
        PathHelperService = serviceProvider.GetService<IPathHelperService>() ??
                            new PathHelperService(modDirectory.FileSystem);
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
        PathHelperService = serviceProvider.GetService<IPathHelperService>() ??
                            new PathHelperService(modDirectory.FileSystem);
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
        Directory = modDirectory;
        PathHelperService = serviceProvider.GetService<IPathHelperService>() ??
                            new PathHelperService(modDirectory.FileSystem);
        InternalPath = CreateInternalPath(modDirectory);
    }

    internal string CreateInternalPath(IDirectoryInfo directory)
    {
        return PathHelperService.NormalizePath(directory.FullName, PathNormalizeOptions.Full);
    }
}