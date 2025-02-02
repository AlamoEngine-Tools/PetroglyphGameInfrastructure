using System;
using System.IO.Abstractions;
using AET.Modinfo;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Represents an ordinary, physical mod.
/// </summary>
public class Mod : ModBase, IPhysicalMod
{ 
    /// <inheritdoc/>
    public IDirectoryInfo Directory { get; }

    /// <summary>
    /// The file system.
    /// </summary>
    protected readonly IFileSystem FileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mod"/> class of the specified mod information.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="modDirectory">The mod's directory.</param>
    /// <param name="workshop">When set to <see langword="true"/> this instance is a Steam Workshop mod.</param>
    /// <param name="name">The name of the mod.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="identifier"/> or <paramref name="modDirectory"/> or <paramref name="name"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> or <paramref name="name"/> is empty.</exception>
    public Mod(IGame game, string identifier, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider)
        : base(game, identifier, workshop ? ModType.Workshops : ModType.Default, name, serviceProvider)
    {
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        DependencyResolveStatus = DependencyResolveStatus.Resolved;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mod"/> class of the specified mod information and modinfo data.
    /// </summary>
    /// <param name="game">The game of the mod.</param>
    /// <param name="identifier">The identifier of the mod.</param>
    /// <param name="modDirectory">The mod's directory.</param>
    /// <param name="workshop">When set to <see langword="true"/> this instance is a Steam Workshop mod.</param>
    /// <param name="modInfoData">The <see cref="IModinfo"/>.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="game"/> or <paramref name="identifier"/> or <paramref name="modDirectory"/> or <paramref name="modInfoData"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> is empty.</exception>
    /// <exception cref="ModinfoException"><paramref name="modInfoData"/> is not valid.</exception>
    public Mod(IGame game, string identifier, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) :
        base(game, identifier, workshop ? ModType.Workshops : ModType.Default, modInfoData, serviceProvider)
    {
        Directory = modDirectory ?? throw new ArgumentNullException(nameof(modDirectory));
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        if (modInfoData.Dependencies.Count == 0)
            DependencyResolveStatus = DependencyResolveStatus.Resolved;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}:{Type} @{Directory.FullName}";
    }
}