using System;
using System.Diagnostics;
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
    /// Creates a new instance
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="tryHandleInitialization">
    /// Indicates whether this instance shall raise the <see cref="InitializationRequested"/>event.
    /// When set to <see langword="false"/> the event will not be raised and initialization cannot be handled.</param>
    protected GameDetectorBase(IServiceProvider serviceProvider, bool tryHandleInitialization)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _tryHandleInitialization = tryHandleInitialization;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        FileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
    }

    /// <summary>
    /// Checks whether a specified directory contains the matching executable file.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <param name="gameType">The <see cref="GameType"/> of the game.</param>
    /// <returns><see langword="true"/>if the directory contains the correct executable file; <see langword="false"/> otherwise.</returns>
    public static bool GameExeExists(IDirectoryInfo directory, GameType gameType)
    {
        if (!directory.Exists)
            return false;

        var exeFile = gameType == GameType.Eaw
            ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

        var exePath = directory.FileSystem.Path.Combine(directory.FullName, exeFile);
        return directory.FileSystem.File.Exists(exePath);
    }

    /// <summary>
    /// Checks whether a specified directory contains the DATA directory together with the MegaFiles.XML file.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns><see langword="true"/>if the directory contains the correct executable file; <see langword="false"/> otherwise.</returns>
    public static bool DataAndMegaFilesXmlExists(IDirectoryInfo directory)
    {
        if (!directory.Exists)
            return false;

        var fs = directory.FileSystem;

        var dataPath = fs.Path.Combine(directory.FullName, "Data");
        if (!fs.Directory.Exists(dataPath))
            return false;

        var megaFilesPath = fs.Path.Combine(dataPath, PetroglyphStarWarsGameConstants.MegaFilesXmlFileName);

        return fs.File.Exists(megaFilesPath);
    }

    /// <inheritdoc/>
    public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
    {
        try
        {
            result = Detect(options);
            if (result.Error is not null)
                return false;
            return result.GameLocation is not null;

        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Unable to find any games, due to error in detection.");
            result = new GameDetectionResult(options.Type, e);
            return false;
        }
    }

    /// <inheritdoc/>
    public GameDetectionResult Detect(GameDetectorOptions options)
    {
        options = options.Normalize();
        var result = GameDetectionResult.NotInstalled(options.Type);

        var locationData = FindGameLocation(options);
        locationData.ThrowIfInvalid();

        if (!locationData.IsInstalled)
        {
            Logger?.LogInformation($"Unable to find an installed game of type {options.Type}.");
            return result;
        }

        if (!HandleInitialization(options, ref locationData))
            return GameDetectionResult.RequiresInitialization(options.Type);

        Debug.Assert(locationData.Location is not null, "Illegal operation state: Expected location to be not null!");

        var location = locationData.Location!;
        var identifier = ServiceProvider.GetRequiredService<IGamePlatformIdentifier>();
        var platform = identifier.GetGamePlatform(options.Type, ref location, options.TargetPlatforms);

        if (!GameExeExists(location, options.Type) || !DataAndMegaFilesXmlExists(location))
        {
            Logger?.LogDebug($"Unable to find the game's executable or megafile.xml at the given location: {location.FullName}");
            return GameDetectionResult.NotInstalled(options.Type);
        }

        if (MatchesOptionsPlatform(options, platform))
        {
            result = new GameDetectionResult(new GameIdentity(options.Type, platform), location);
            Logger?.LogInformation($"Game detected: {result.GameIdentity} at location: {location.FullName}");
            return result;
        }

        Logger?.LogInformation($"Game detected at location: {result.GameLocation?.FullName} " +
                               $"but Platform {platform} was not requested.");
        return result;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetType().Name;
    }

    /// <summary>
    /// Instance specific implementation which tries to find a game installation. 
    /// </summary>
    /// <param name="options">The search options</param>
    /// <returns>Information about a found game installation.</returns>
    /// <remarks>This method may throw arbitrary exceptions.</remarks>
    protected internal abstract GameLocationData FindGameLocation(GameDetectorOptions options);

    private static bool MatchesOptionsPlatform(GameDetectorOptions options, GamePlatform identifiedPlatform)
    {
        return options.TargetPlatforms.Contains(GamePlatform.Undefined) ||
               options.TargetPlatforms.Contains(identifiedPlatform);
    }

    private bool HandleInitialization(GameDetectorOptions options, ref GameLocationData locationData)
    {
        if (!locationData.InitializationRequired)
            return true;

        Logger?.LogDebug($"It appears that the game '{locationData.ToString()}' exists but it is not initialized. Game options: {options}.");
        if (!_tryHandleInitialization)
            return false;

        if (RequestInitialization(options))
            locationData = FindGameLocation(options);

        return locationData.Location is not null;
    }

    private bool RequestInitialization(GameDetectorOptions options)
    {
        var request = new GameInitializeRequestEventArgs(options);
        var callbacks = InitializationRequested;
        if (callbacks is not null)
        {
            Logger?.LogTrace("Calling event handler to initialize and try to get location again...");
            callbacks.Invoke(this, request);
        }
        return request.Handled;
    }

    /// <summary>
    /// Represents location and initialization state of a game.
    /// </summary>
    protected internal readonly struct GameLocationData
    {
        /// <summary>
        /// Nullable location entry.
        /// </summary>
        public IDirectoryInfo? Location { get; init; }

        /// <summary>
        /// Indicates whether an initialization is required.
        /// </summary>
        public bool InitializationRequired { get; init; }

        /// <summary>
        /// Indicates whether this instance represents an installed game.
        /// </summary>
        public bool IsInstalled => Location != null || InitializationRequired;

        internal void ThrowIfInvalid()
        {
            if (Location is not null && InitializationRequired)
                throw new NotSupportedException("The LocationData cannot have a location set " +
                                                $"but also {nameof(InitializationRequired)} set to true.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (InitializationRequired)
                return "<INIT REQUIRED>";
            return Location is not null ? "<GAME NOT INSTALLED>" : Location!.FullName;
        }
    }
}