using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// A <see cref="IGameDetector"/> which take multiple other detectors in a sorted order and searches for a game installation.
/// </summary>
public sealed class CompositeGameDetector : IGameDetector
{
    /// <inheritdoc/>
    public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

    private readonly bool _disposeDetectors;

    /// <summary>
    /// Gets a sorted list of the detectors to use.
    /// </summary>
    public IReadOnlyList<IGameDetector> SortedDetectors { get; }

    private readonly ILogger? _logger;

    /// <summary>
    /// Creates a new instance with a given list of <see cref="IGameDetector"/>s to use.
    /// </summary>
    /// <param name="sortedDetectors">The sorted list of detectors which shall get used.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="disposeDetectors">
    /// When <see langword="true"/>after a detector was used, it will get disposed if it implements <see cref="IDisposable"/>.
    /// Default is <see langword="false"/>
    /// </param>
    public CompositeGameDetector(IList<IGameDetector> sortedDetectors, IServiceProvider serviceProvider, bool disposeDetectors = false)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        ThrowHelper.ThrowIfCollectionNullOrEmptyOrContainsNull(sortedDetectors);
        SortedDetectors = sortedDetectors.ToList();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _disposeDetectors = disposeDetectors;
    }

    /// <summary>
    /// Runs the <see cref="SortedDetectors"/> and returns the first found game installation.
    /// Errors in each internal detector will be aggregated and returned with the result.
    /// </summary>
    /// <param name="gameType">The game type to detect.</param>
    /// <param name="platforms">Collection of the platforms to search for.</param>
    /// <returns>Data which holds the game's location or error information.</returns>
    /// <exception cref="AggregateException"></exception>
    public GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms)
    {
        var errors = new List<Exception>();
        GameDetectionResult? lastResult = null;
        foreach (var detector in SortedDetectors)
        {
            _logger?.LogDebug($"Searching for game {gameType} with detector: {detector}");
            detector.InitializationRequested += PassThroughInitializationRequest;
            
            try
            {
                var result = detector.Detect(gameType, platforms);
                if (result is not null && result.Installed)
                    return result;
                lastResult = result;
            }
            catch (Exception e)
            {
                _logger?.LogTrace($"Failed detecting game using detector {detector}. {e}");
                errors.Add(e);

                if (detector.Equals(SortedDetectors[SortedDetectors.Count - 1]))
                    throw new AggregateException(errors);
            }
            finally
            {
                detector.InitializationRequested -= PassThroughInitializationRequest;
                if (_disposeDetectors && detector is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        if (lastResult is not null)
            return lastResult;

        _logger?.LogTrace("No detector produced a result, but also none crashed.");
        return GameDetectionResult.NotInstalled(gameType);

    }

    /// <summary>
    /// Runs the <see cref="SortedDetectors"/> and returns the first found game installation.
    /// Errors in each internal detector will be aggregated and returned with the result.
    /// </summary>
    /// <param name="platforms"></param>
    /// <param name="result">Data which holds the game's location or error information.</param>
    /// <param name="gameType"></param>
    /// <returns><see langword="true"/> when a game was found; <see langword="false"/> otherwise.</returns>
    public bool TryDetect(GameType gameType, ICollection<GamePlatform> platforms, out GameDetectionResult result)
    {
        try
        {
            result = Detect(gameType, platforms);
            return result.Installed;
        }
        catch (Exception e)
        {
            _logger?.LogWarning(e, "Unable to find any games, due to error in detection.");
            result = GameDetectionResult.NotInstalled(gameType);
            return false;
        }
    }

    private void PassThroughInitializationRequest(object? sender, GameInitializeRequestEventArgs e)
    {
        OnInitializationRequested(e);
    }

    private void OnInitializationRequested(GameInitializeRequestEventArgs e)
    {
        InitializationRequested?.Invoke(this, e);
    }
}