using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.FileService;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Services.Language;
using Validation;

namespace PetroGlyph.Games.EawFoc.Games;

/// <summary>
/// Represents a Petroglyph Star War Game, which is either Empire at War or Forces of Corruption.
/// </summary>
public class PetroglyphStarWarsGame : PlayableObject, IGame
{
    /// <inheritdoc/>
    public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;

    private readonly string _normalizedPath;
    private IPhysicalFileService? _fileService;
    private IDirectoryInfo? _modLocation;

    /// <summary>
    /// Service for handling file system paths.
    /// </summary>
    protected readonly IPathHelperService PathHelperService;

    /// <summary>
    /// Shared Service provider instance.
    /// </summary>
    protected IServiceProvider ServiceProvider;

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
    public IFileSystem FileSystem => Directory.FileSystem;

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

    /// <inheritdoc/>
    public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

    /// <summary>
    /// Creates a new game instance.
    /// </summary>
    /// <param name="gameIdentity"></param>
    /// <param name="gameDirectory"></param>
    /// <param name="name"></param>
    /// <param name="serviceProvider"></param>
    public PetroglyphStarWarsGame(
        IGameIdentity gameIdentity,
        IDirectoryInfo gameDirectory,
        string name,
        IServiceProvider serviceProvider)
    {
        Requires.NotNullAllowStructs(gameIdentity, nameof(gameIdentity));
        Requires.NotNullOrEmpty(name, nameof(name));
        Requires.NotNull(gameDirectory, nameof(gameDirectory));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        Name = name;
        Type = gameIdentity.Type;
        Platform = gameIdentity.Platform;
        Directory = gameDirectory;
        ServiceProvider = serviceProvider;
        PathHelperService = serviceProvider.GetService<IPathHelperService>() ??
                            new PathHelperService(gameDirectory.FileSystem);
        _normalizedPath = PathHelperService.NormalizePath(Directory.FullName, PathNormalizeOptions.Full);
    }

    /// <inheritdoc/>
    public virtual bool Exists()
    {
        try
        {
            var d = new DirectoryGameDetector(Directory, ServiceProvider);
            var r = d.FindGameLocation(new GameDetectorOptions(Type) { TargetPlatforms = new[] { Platform } });
            return r.IsInstalled && r.Location == Directory;
        }
        catch (IOException)
        {
            return false;
        }
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
        var normalizedDirectory =
            PathHelperService.NormalizePath(other.Directory.FullName, PathNormalizeOptions.Full);
        return _normalizedPath.Equals(normalizedDirectory, StringComparison.Ordinal);
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
        unchecked
        {
            var hashCode = _normalizedPath.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)Type;
            hashCode = (hashCode * 397) ^ (int)Platform;
            return hashCode;
        }
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
        return ServiceProvider.GetService<IGameLanguageFinder>()?
                   .FindInstalledLanguages(this) ??
               new HashSet<ILanguageInfo>();
    }

    /// <summary>
    /// Resolves the icon of this instance.
    /// </summary>
    /// <returns>Resolve icon path or <see langword="null"/>.</returns>
    protected override string? ResolveIconFile()
    {
        var finder = ServiceProvider.GetService<IGameIconFinder>() ?? new FallbackGameIconFinder();
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