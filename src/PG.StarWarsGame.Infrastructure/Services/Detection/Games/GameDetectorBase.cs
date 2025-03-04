using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Base implementation for an <see cref="IGameDetector"/>
/// </summary>
public abstract class GameDetectorBase : IGameDetector
{
    /// <inheritdoc/>
    public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

    /// <summary>
    /// Instance shared Service Provider.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    private readonly bool _tryHandleInitialization;

    /// <summary>
    /// Instance shared logger.
    /// </summary>
    protected ILogger? Logger;

    /// <summary>
    /// Instance shared file system.
    /// </summary>
    protected IFileSystem FileSystem;

    /// <summary>
    /// Creates a new instance of the <see cref="GameDetectorBase"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="tryHandleInitialization">
    /// Indicates whether this instance shall raise the <see cref="InitializationRequested"/> event.
    /// When set to <see langword="false"/> the event will not be raised and initialization cannot be handled.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    protected GameDetectorBase(IServiceProvider serviceProvider, bool tryHandleInitialization)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _tryHandleInitialization = tryHandleInitialization;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }
    
    /// <inheritdoc/>
    public bool TryDetect(GameType gameType, ICollection<GamePlatform> platforms, out GameDetectionResult result)
    {
        try
        {
            result = Detect(gameType, platforms);
            return result.Installed;
        }
        catch (Exception e)
        {
            Logger?.LogDebug(e, "Unable to find any games, due to error in detection.");
            result = GameDetectionResult.NotInstalled(gameType);
            return false;
        }
    }

    /// <inheritdoc/>
    public GameDetectionResult Detect(GameType gameType, params ICollection<GamePlatform> platforms)
    {
        platforms = NormalizePlatforms(platforms);

        var locationData = FindGameLocation(gameType);

        if (!locationData.IsInstalled)
        {
            Logger?.LogTrace($"Unable to find an installed game of type {gameType}.");
            return GameDetectionResult.NotInstalled(gameType);
        }

        if (!HandleInitialization(gameType, ref locationData))
            return GameDetectionResult.RequiresInitialization(gameType);

        var location = locationData.Location!;
        var identifier = ServiceProvider.GetRequiredService<IGamePlatformIdentifier>();
        var actualPlatform = identifier.GetGamePlatform(gameType, ref location);

        // This is the bare minimum for a game to exist on the disk. If we don't have these files,
        // the detector returned a false result.
        if (!GameExeExists(location, gameType) || !DataAndMegaFilesXmlExists(location))
        {
            Logger?.LogTrace($"Unable to find the game's executable or megafiles.xml at the given location: {location.FullName}");
            return GameDetectionResult.NotInstalled(gameType);
        }

        if (!MatchesOptionsPlatform(platforms, actualPlatform))
        {
            var wrongGameFound = GameDetectionResult.NotInstalled(gameType);
            Logger?.LogTrace($"Game detected at location: {wrongGameFound.GameLocation?.FullName} " +
                                   $"but Platform {actualPlatform} was not requested.");
            return wrongGameFound;
        }

        var detectedResult = GameDetectionResult.FromInstalled(new GameIdentity(gameType, actualPlatform), location);
        Logger?.LogDebug($"Game detected: {detectedResult.GameIdentity} at location: '{location.FullName}'");
        return detectedResult;
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return GetType().Name;
    }

    internal static bool MinimumGameFilesExist(GameType type, IDirectoryInfo directory)
    {
        return GameExeExists(directory, type) && DataAndMegaFilesXmlExists(directory);
    }


    internal static bool GameExeExists(IDirectoryInfo directory, GameType gameType)
    {
        if (!directory.Exists)
            return false;

        var exeFile = gameType == GameType.Eaw
            ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

        var exePath = directory.FileSystem.Path.Combine(directory.FullName, exeFile);
        return directory.FileSystem.File.Exists(exePath);
    }

    internal static bool DataAndMegaFilesXmlExists(IDirectoryInfo directory)
    {
        var fs = directory.FileSystem;

        var dataPath = fs.Path.Combine(directory.FullName, "Data");
        if (!fs.Directory.Exists(dataPath))
            return false;

        var megaFilesPath = fs.Path.Combine(dataPath, PetroglyphStarWarsGameConstants.MegaFilesXmlFileName);

        return fs.File.Exists(megaFilesPath);
    }

    /// <summary>
    /// Instance specific implementation which tries to find a game installation. 
    /// </summary>
    /// <param name="gameType">The game type to detect.</param>
    /// <returns>Information about a found game installation.</returns>
    /// <remarks>This method may throw arbitrary exceptions.</remarks>
    protected abstract GameLocationData FindGameLocation(GameType gameType);

    private static bool MatchesOptionsPlatform(ICollection<GamePlatform> platforms, GamePlatform identifiedPlatform)
    {
        return platforms.Contains(GamePlatform.Undefined) ||
               platforms.Contains(identifiedPlatform);
    }

    private bool HandleInitialization(GameType gameType, ref GameLocationData locationData)
    {
        if (!locationData.InitializationRequired)
            return true;

        Logger?.LogDebug($"It appears that the game '{locationData}' exists but it is not initialized. Game type '{gameType}'.");
        if (!_tryHandleInitialization)
            return false;

        if (RequestInitialization(gameType))
            locationData = FindGameLocation(gameType);

        return locationData.Location is not null;
    }

    private bool RequestInitialization(GameType gameType)
    {
        var request = new GameInitializeRequestEventArgs(gameType);
        var callbacks = InitializationRequested;
        if (callbacks is not null)
        {
            Logger?.LogTrace("Calling event handler to initialize and try to get location again...");
            callbacks.Invoke(this, request);
        }
        return request.Handled;
    }

    private static HashSet<GamePlatform> NormalizePlatforms(ICollection<GamePlatform> platforms)
    {
        if (platforms.Count == 0 || platforms.Contains(GamePlatform.Undefined))
            return [GamePlatform.Undefined];
        return [..platforms];
    }

    /// <summary>
    /// Represents location and initialization state of a game.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GameLocationData"/> struct of the specified game location.
    /// </remarks>
    /// <param name="location">The detected game location or <see langword="null"/> if no game was detected.</param>
    public readonly struct GameLocationData(IDirectoryInfo? location)
    {
        /// <summary>
        /// Gets a <see cref="GameLocationData"/> representing a not installed location.
        /// </summary>
        public static readonly GameLocationData NotInstalled = default;

        /// <summary>
        /// Gets a <see cref="GameLocationData"/> representing an uninitialized game state.
        /// </summary>
        public static readonly GameLocationData RequiresInitialization = new() { InitializationRequired = true };

        /// <summary>
        /// Nullable location entry.
        /// </summary>
        public IDirectoryInfo? Location { get; } = location;

        /// <summary>
        /// Indicates whether an initialization is required.
        /// </summary>
        public bool InitializationRequired { get; private init; }

        /// <summary>
        /// Indicates whether this instance represents an installed game.
        /// </summary>
        public bool IsInstalled => Location != null || InitializationRequired;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (InitializationRequired)
                return "<INIT REQUIRED>";
            return Location is not null ? "<GAME NOT INSTALLED>" : Location!.FullName;
        }
    }
}