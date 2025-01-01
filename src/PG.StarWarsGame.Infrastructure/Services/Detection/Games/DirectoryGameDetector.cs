using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// A <see cref="IGameDetector"/> that that is able to detect game installations from a specified directory.
/// </summary>
/// <remarks>
/// This detector does not support game initialization requests.
/// </remarks>
public sealed class DirectoryGameDetector : GameDetectorBase
{
    private readonly IDirectoryInfo _directory;

    /// <summary>
    /// Creates a new instance of the <see cref="DirectoryGameDetector"/> class.
    /// </summary>
    /// <param name="directory">The directory to search for an installation.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public DirectoryGameDetector(IDirectoryInfo directory, IServiceProvider serviceProvider) : base(serviceProvider, false)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    /// <inheritdoc/>
    protected override GameLocationData FindGameLocation(GameType gameType)
    {
        Logger?.LogDebug($"Searching for game {gameType} at directory: {_directory}");
        return !MinimumGameFilesExist(gameType, _directory) ? GameLocationData.NotInstalled : new GameLocationData(_directory);
    }
}