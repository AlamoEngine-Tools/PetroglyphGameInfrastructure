using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Detects whether a given directory contains a Petroglyph Star Wars Game
/// </summary>
public sealed class DirectoryGameDetector : GameDetectorBase
{
    private readonly IDirectoryInfo _directory;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="directory">The directory to search for an installation.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public DirectoryGameDetector(IDirectoryInfo directory, IServiceProvider serviceProvider) : base(serviceProvider, false)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    /// <inheritdoc/>
    protected internal override GameLocationData FindGameLocation(GameDetectorOptions options)
    {
        Logger?.LogDebug($"Searching for game {options.Type} at directory: {_directory}");

        if (GameExeExists(_directory, options.Type) && DataAndMegaFilesXmlExists(_directory))
            return new GameLocationData { Location = _directory };
        return default;
    }
}