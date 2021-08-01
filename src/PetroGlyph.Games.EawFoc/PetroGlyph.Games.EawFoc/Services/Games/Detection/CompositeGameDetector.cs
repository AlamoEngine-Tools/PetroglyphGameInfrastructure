using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Game detector which take multiple other detectors in a sorted order and searches for a game installation.
    /// </summary>
    public sealed class CompositeGameDetector : IGameDetector
    {
        /// <inheritdoc/>
        public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

        private readonly bool _disposeDetectors;

        /// <summary>
        /// Sorted list of the internal detectors
        /// </summary>
        public IList<IGameDetector> SortedDetectors { get; }

        private readonly ILogger? _logger;

        /// <summary>
        /// Creates a new instance with a given list of <see cref="IGameDetector"/>s to use.
        /// </summary>
        /// <param name="sortedDetectors">The sorted list of detectors which shall get used.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="disposeDetectors">When <see langword="true"/>after a detector was used, it will get disposed if it implements <see cref="IDisposable"/>.
        /// Default is <see langword="false"/></param>
        public CompositeGameDetector(IList<IGameDetector> sortedDetectors, IServiceProvider serviceProvider, bool disposeDetectors = false)
        {
            Requires.NotNullOrEmpty(sortedDetectors, nameof(sortedDetectors));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            SortedDetectors = sortedDetectors;
            _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
            _disposeDetectors = disposeDetectors;
        }

        /// <summary>
        /// Runs the <see cref="SortedDetectors"/> and returns the first found game installation.
        /// Errors in each internal detector will be aggregated and returned with the result.
        /// </summary>
        /// <param name="options">The search query.</param>
        /// <returns>The search result.</returns>
        public GameDetectionResult Detect(GameDetectorOptions options)
        {
            var errors = new List<Exception>();
            foreach (var sortedDetector in SortedDetectors)
            {
                _logger?.LogDebug($"Searching for game {options.Type} with detector: {sortedDetector}");
                sortedDetector.InitializationRequested += PassThroughInitializationRequest;
                var result = sortedDetector.Detect(options);
                sortedDetector.InitializationRequested -= PassThroughInitializationRequest;
                if (_disposeDetectors && sortedDetector is IDisposable disposable)
                    disposable.Dispose();
                if (result.GameLocation is not null)
                    return result;
                if (result.Error is not null)
                    errors.Add(result.Error);
            }

            return errors.Any()
                ? new GameDetectionResult(options.Type, new AggregateException(errors))
                : GameDetectionResult.NotInstalled(options.Type);
        }

        /// <summary>
        /// Runs the <see cref="SortedDetectors"/> and returns the first found game installation.
        /// Errors in each internal detector will be aggregated and returned with the result.
        /// </summary>
        /// <param name="options">Provided search options.</param>
        /// <param name="result">Data which holds the game's location or error information.</param>
        /// <returns><see langword="true"/> when a game was found; <see langword="false"/> otherwise.</returns>
        public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
        {
            result = Detect(options);
            if (result.Error is not null)
                return false;
            return result.GameLocation is not null;
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
}