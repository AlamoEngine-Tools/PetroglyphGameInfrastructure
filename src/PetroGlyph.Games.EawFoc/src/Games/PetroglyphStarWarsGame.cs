using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Services.Language;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Represents a Petroglyph Star War Game, which is either Empire at War or Forces of Corruption.
/// </summary>
public class PetroglyphStarWarsGame : PlayableModContainer, IGame
{
    private readonly string _normalizedPath;
    private IDirectoryInfo? _modLocation;
    
    /// <summary>
    /// Gets the file system of this game.
    /// </summary>
    protected readonly IFileSystem FileSystem;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    public override IGame Game => this;

    /// <inheritdoc/>
    public GameType Type { get; }

    /// <inheritdoc/>
    public GamePlatform Platform { get; }

    /// <inheritdoc/>
    public IDirectoryInfo Directory { get; }

    /// <inheritdoc/>
    public IDirectoryInfo ModsLocation
    {
        get
        {
            if (_modLocation is null)
            {
                var fs = Directory.FileSystem;
                var modsPath = fs.Path.Combine(Directory.FullName, "Mods");
                _modLocation = fs.DirectoryInfo.New(modsPath);
            }
            return _modLocation;
        }
    }

    /// <summary>
    /// Initializes a new game instance of the <see cref="PetroglyphStarWarsGame"/> class with the specified identity, name and location.
    /// </summary>
    /// <param name="gameIdentity">The game's identity.</param>
    /// <param name="gameDirectory">The game's install directory</param>
    /// <param name="name">The name of the game.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="gameIdentity"/> or <paramref name="gameDirectory"/> or <paramref name="name"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public PetroglyphStarWarsGame(
        IGameIdentity gameIdentity,
        IDirectoryInfo gameDirectory,
        string name,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (gameDirectory == null) 
            throw new ArgumentNullException(nameof(gameDirectory));
        if (gameIdentity is null)
            throw new ArgumentNullException(nameof(gameIdentity));
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);

        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Name = name;
        Type = gameIdentity.Type;
        Platform = gameIdentity.Platform;
        Directory = gameDirectory;
        ServiceProvider = serviceProvider;
        _normalizedPath = PathNormalizer.Normalize(FileSystem.Path.GetFullPath(Directory.FullName), new PathNormalizeOptions
        {
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim,
            UnifyCase = UnifyCasingKind.UpperCase
        });
    }

    /// <inheritdoc/>
    public virtual bool Exists()
    {
        return GameDetectorBase.MinimumGameFilesExist(Type, Directory);
    }

    /// <inheritdoc/>
    public bool Equals(IGame? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (Type != other.Type)
            return false;
        if (Platform != other.Platform)
            return false;
        var normalizedDirectory = PathNormalizer.Normalize(FileSystem.Path.GetFullPath(other.Directory.FullName), new PathNormalizeOptions
        {
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim,
            UnifyCase = UnifyCasingKind.UpperCase
        });
        return _normalizedPath.Equals(normalizedDirectory, StringComparison.Ordinal);
    }

    bool IEquatable<IGameIdentity>.Equals(IGameIdentity other)
    {
        return Type == other.Type && Platform == other.Platform;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (this == obj)
            return true;
        if (obj is null)
            return false;
        return obj is PetroglyphStarWarsGame otherGame && Equals(otherGame);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_normalizedPath, Type, Platform);
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{Type}:{Platform} @{Directory}";
    }
}