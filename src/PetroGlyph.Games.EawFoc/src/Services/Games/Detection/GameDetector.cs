using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

/// <summary>
/// Base implementation for an <see cref="IGameDetector"/>
/// </summary>
public abstract class GameDetector : IGameDetector
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
    protected GameDetector(IServiceProvider serviceProvider, bool tryHandleInitialization)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        _tryHandleInitialization = tryHandleInitialization;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        FileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
    }

    /// <summary>
    /// Checks whether a given directory contains the matching executable files.
    /// </summary>
    /// <param name="directory">The target directory</param>
    /// <param name="gameType">The target <see cref="GameType"/></param>
    /// <returns><see langword="true"/>if the directory contains the correct executable file; <see langword="false"/> otherwise.</returns>
    public static bool GameExeExists(IDirectoryInfo directory, GameType gameType)
    {
        if (!directory.Exists)
            return false;

        var exeFile = gameType == GameType.EaW
            ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

        var exePath = directory.FileSystem.Path.Combine(directory.FullName, exeFile);
        return directory.FileSystem.File.Exists(exePath);
    }

    /// <inheritdoc/>
    public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
    {
        result = Detect(options);
        if (result.Error is not null)
            return false;
        return result.GameLocation is not null;
    }

    /// <inheritdoc/>
    public GameDetectionResult Detect(GameDetectorOptions options)
    {
        options = options.Normalize();
        var result = GameDetectionResult.NotInstalled(options.Type);
        try
        {
            var locationData = FindGameLocation(options);
            locationData.ThrowIfInvalid();

            if (!locationData.IsInstalled)
            {
                Logger?.LogInformation($"Unable to find an installed game of type {options.Type}.");
                return result;
            }

            if (!HandleInitialization(options, ref locationData))
                return GameDetectionResult.RequiresInitialization(options.Type);

#if DEBUG
            if (locationData.Location is null)
                throw new InvalidOperationException("Illegal operation state: Expected location to be not null!");
#endif

            var location = locationData.Location!;
            var identifier = ServiceProvider.GetService<IGamePlatformIdentifier>() ?? new GamePlatformIdentifier(ServiceProvider);
            var platform = identifier.GetGamePlatform(options.Type, ref location, options.TargetPlatforms);

            if (!GameExeExists(location, options.Type))
            {
                Logger?.LogDebug($"Unable to find any game executables at the given location: {location.FullName}");
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
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Unable to find any games, due to error in detection");
            return new GameDetectionResult(options.Type, e);
        }
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

        Logger?.LogInformation("The games seems to exists but is not initialized.");
        if (!_tryHandleInitialization)
            return false;

        Logger?.LogInformation("Calling event handler to initialize and try to get location again....");
        if (RequestInitialization(options))
            locationData = FindGameLocation(options);

        return locationData.Location is not null;
    }

    private bool RequestInitialization(GameDetectorOptions options)
    {
        var request = new GameInitializeRequestEventArgs(options);
        InitializationRequested?.Invoke(this, request);
        return request.Handled;
    }

    /// <summary>
    /// Internal state struct for representing the result of the abstract <see cref="GameDetector.FindGameLocation"/> method.
    /// </summary>
    protected internal struct GameLocationData
    {
        /// <summary>
        /// Nullable location entry.
        /// </summary>
        public IDirectoryInfo? Location;

        /// <summary>
        /// Indicates whether an initialization is required.
        /// </summary>
        public bool InitializationRequired;

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
    }
}