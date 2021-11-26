using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

/// <summary>
/// Detects whether a given directory contains a Petroglyph Star Wars Game
/// </summary>
public sealed class DirectoryGameDetector : GameDetector
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
        Requires.NotNull(directory, nameof(directory));
        _directory = directory;
    }

    /// <summary>
    /// Gets an instance which checks the current process' location for an installation.
    /// </summary>
    /// <param name="serviceProvider">the service provider which shall be passed to the instance.</param>
    /// <returns>The detector instance.</returns>
    public static DirectoryGameDetector CurrentDirectoryGameDetector(IServiceProvider serviceProvider)
    {
        var fs = new FileSystem();
        var currentDir = fs.DirectoryInfo.FromDirectoryName(fs.Directory.GetCurrentDirectory());
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
#if NET
            {
                GameType.EaW => _directory.EnumerateDirectories(KnownEawSubDirName,
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true }),
                GameType.Foc => _directory.EnumerateDirectories("*",
                        new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
                    .Where(d => KnownFocDirectoryNames.Contains(d.Name)),
                _ => throw new ArgumentOutOfRangeException()
            };
#else
        {
            GameType.EaW => _directory.EnumerateDirectories(KnownEawSubDirName, SearchOption.AllDirectories),
            GameType.Foc => _directory.EnumerateDirectories()
                .Where(d => KnownFocDirectoryNames.Contains(d.Name)),
            _ => throw new ArgumentOutOfRangeException()
        };
#endif
        return subDirectories.FirstOrDefault();
    }
}