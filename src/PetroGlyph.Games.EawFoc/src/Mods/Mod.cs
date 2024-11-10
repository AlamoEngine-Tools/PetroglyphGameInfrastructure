using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// An ordinary, physical mod.
/// </summary>
public class Mod : ModBase, IPhysicalMod
{ 
    private string? _identifier;

    internal string InternalPath { get; }

    /// <inheritdoc/>
    public IDirectoryInfo Directory { get; }

    /// <summary>
    /// Gets the file system.
    /// </summary>
    protected readonly IFileSystem FileSystem;

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
    /// <param name="name">The name of the mod.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="modDirectory"/> or <paramref name="name"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider)
        : base(game, workshop ? ModType.Workshops : ModType.Default, name, serviceProvider)
    {
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="modDirectory"/> or <paramref name="modInfoData"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) :
        base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData, serviceProvider)
    {
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        InternalPath = CreateInternalPath(modDirectory);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}:{Type} @{Directory.FullName}";
    }

    internal string CreateInternalPath(IDirectoryInfo directory)
    {
        return FileSystem.Path.GetFullPath(directory.FullName);
    }
}