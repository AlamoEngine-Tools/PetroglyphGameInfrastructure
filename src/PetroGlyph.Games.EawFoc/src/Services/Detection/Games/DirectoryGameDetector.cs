﻿using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
/* Nicht gemergte Änderung aus Projekt "PG.StarWarsGame.Infrastructure (net8.0)"
Vor:
using PG.StarWarsGame.Infrastructure.Games;
Nach:
using PG;
using PG.StarWarsGame;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.StarWarsGame.Infrastructure.Services.Detection.Games;
*/

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Detects whether a given directory contains a Petroglyph Star Wars Game
/// </summary>
public sealed class DirectoryGameDetector : GameDetectorBase
{
    private const string KnownEawSubDirName = "GameData";
    private static readonly string[] KnownFocDirectoryNames = { "EAWX", "corruption", "Star Wars Empire at War Forces of Corruption" };

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

    /// <summary>
    /// Gets an instance which checks the current process' location for an installation.
    /// </summary>
    /// <param name="serviceProvider">the service provider which shall be passed to the instance.</param>
    /// <returns>The detector instance.</returns>
    public static DirectoryGameDetector CurrentDirectoryGameDetector(IServiceProvider serviceProvider)
    {
        var fs = new FileSystem();
        var currentDir = fs.DirectoryInfo.New(fs.Directory.GetCurrentDirectory());
        return new DirectoryGameDetector(currentDir, serviceProvider);
    }

    /// <inheritdoc/>
    protected internal override GameLocationData FindGameLocation(GameDetectorOptions options)
    {
        Logger?.LogDebug($"Searching for game {options.Type} at directory: {_directory}");
        var subDirectory = FindSuitableSubDirectory(options.Type);
        if (subDirectory is not null && GameExeExists(subDirectory, options.Type))
            return new GameLocationData { Location = subDirectory };

        return !GameExeExists(_directory, options.Type)
            ? default
            : new GameLocationData { Location = _directory };
    }

    private IDirectoryInfo? FindSuitableSubDirectory(GameType type)
    {
        var subDirectories = type switch
#if NET || NETSTANDARD2_1_OR_GREATER
        {
            GameType.Eaw => _directory.EnumerateDirectories(KnownEawSubDirName,
                new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true }),
            GameType.Foc => _directory.EnumerateDirectories("*",
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
                .Where(d => KnownFocDirectoryNames.Contains(d.Name)),
            _ => throw new ArgumentOutOfRangeException()
        };
#else
        {
            GameType.Eaw => _directory.EnumerateDirectories(KnownEawSubDirName, SearchOption.AllDirectories),
            GameType.Foc => _directory.EnumerateDirectories()
                .Where(d => KnownFocDirectoryNames.Contains(d.Name)),
            _ => throw new ArgumentOutOfRangeException()
        };
#endif
        return subDirectories.FirstOrDefault();
    }
}