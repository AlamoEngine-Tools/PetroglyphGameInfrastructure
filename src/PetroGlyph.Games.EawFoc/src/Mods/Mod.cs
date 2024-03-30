using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.FileService;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// An ordinary, physical mod.
/// </summary>
public class Mod : ModBase, IPhysicalMod
{
    private IPhysicalFileService? _fileService;
    private string? _identifier;

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
            _fileService = new DefaultFileService(this);
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
                var identifierBuilder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
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
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        ModinfoFile = modinfoFile;
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
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
        if (modDirectory == null) 
            throw new ArgumentNullException(nameof(modDirectory));
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
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
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
        InternalPath = CreateInternalPath(modDirectory);
    }

    internal string CreateInternalPath(IDirectoryInfo directory)
    {
        return FileSystem.Path.GetFullPath(directory.FullName);
    }
}