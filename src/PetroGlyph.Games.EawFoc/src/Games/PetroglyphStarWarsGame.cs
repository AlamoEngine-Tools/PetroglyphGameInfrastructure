using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Icon;
using PG.StarWarsGame.Infrastructure.Services.Language;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Represents a Petroglyph Star War Game, which is either Empire at War or Forces of Corruption.
/// </summary>
public class PetroglyphStarWarsGame : PlayableObject, IGame
{
    /// <inheritdoc/>
    public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;

    private readonly string _normalizedPath;
    private IDirectoryInfo? _modLocation;
    
    /// <summary>
    /// Gets the service provider.
    /// </summary>
    protected IServiceProvider ServiceProvider;

    /// <summary>
    /// Gets the file system of this game.
    /// </summary>
    protected readonly IFileSystem FileSystem;

    /// <summary>
    /// Shared internal (modifiable) set of mods.
    /// </summary>
    protected internal readonly HashSet<IMod> ModsInternal = new();

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

    /// <inheritdoc/>
    public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

    /// <summary>
    /// Initializes a new game instance of the <see cref="PetroglyphStarWarsGame"/> class with the specified identity, name and location.
    /// </summary>
    /// <param name="gameIdentity">The game's identity.</param>
    /// <param name="gameDirectory">The game's install directory</param>
    /// <param name="name">The name of the game.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public PetroglyphStarWarsGame(
        IGameIdentity gameIdentity,
        IDirectoryInfo gameDirectory,
        string name,
        IServiceProvider serviceProvider)
    {
        if (gameDirectory == null) 
            throw new ArgumentNullException(nameof(gameDirectory));
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
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
    public virtual bool AddMod(IMod mod)
    {
        // To avoid programming errors due to copies of the same game instance, we only check for reference equality.
        if (!ReferenceEquals(this, mod.Game))
            throw new ModException(mod, "Mod does not match to this game instance.");

        var result = ModsInternal.Add(mod);
        if (result)
            OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
        return result;
    }

    /// <inheritdoc/>
    public virtual bool RemoveMod(IMod mod)
    {
        var result = ModsInternal.Remove(mod);
        if (result)
            OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
        return result;
    }

    /// <inheritdoc/>
    public IMod FindMod(IModReference modReference)
    {
        var identifierBuilder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
        var normalized = identifierBuilder.Normalize(modReference);
        var foundMod = Mods.FirstOrDefault(normalized.Equals);
        if (foundMod is null)
            throw new ModNotFoundException(modReference, this);
        return foundMod;
    }

    /// <inheritdoc/>
    public bool TryFindMod(IModReference modReference, out IMod? mod)
    {
        mod = null;
        try
        {
            mod = FindMod(modReference);
            return true;
        }
        catch (ModNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IEnumerator<IMod> GetEnumerator()
    {
        return Mods.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
        return obj is IGame otherGame && Equals(otherGame);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_normalizedPath, Type, Platform);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type}:{Platform} @{Directory}";
    }

    /// <summary>
    /// Resolves the game's installed languages.
    /// </summary>
    /// <returns>Set of resolved languages.</returns>
    protected override ISet<ILanguageInfo> ResolveInstalledLanguages()
    {
        return ServiceProvider.GetRequiredService<IGameLanguageFinder>().FindInstalledLanguages(this);
    }

    /// <summary>
    /// Resolves the icon of this instance.
    /// </summary>
    /// <returns>Resolve icon path or <see langword="null"/>.</returns>
    protected override string? ResolveIconFile()
    {
        var finder = ServiceProvider.GetService<IGameIconFinder>() ?? new FallbackGameIconFinder(ServiceProvider);
        return finder.FindIcon(this);
    }

    /// <summary>
    /// Raises the <see cref="ModsCollectionModified"/> event for this instance.
    /// </summary>
    /// <param name="e">The event args to pass.</param>
    protected virtual void OnModsCollectionModified(ModCollectionChangedEventArgs e)
    {
        ModsCollectionModified?.Invoke(this, e);
    }
}