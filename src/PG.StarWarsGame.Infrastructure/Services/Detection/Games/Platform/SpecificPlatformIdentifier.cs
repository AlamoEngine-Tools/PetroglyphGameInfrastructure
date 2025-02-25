﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal abstract class SpecificPlatformIdentifier : ISpecificPlatformIdentifier
{
    protected readonly ILogger? Logger;

    protected SpecificPlatformIdentifier(IServiceProvider serviceProvider)
    {
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public bool IsPlatform(GameType type, ref IDirectoryInfo location)
    {
        return type == GameType.Eaw ? IsPlatformEaw(ref location) : IsPlatformFoc(ref location);
    }

    public abstract bool IsPlatformFoc(ref IDirectoryInfo location);

    public abstract bool IsPlatformEaw(ref IDirectoryInfo location);


    protected static bool DirectoryContainsFiles(IDirectoryInfo directory, ICollection<string> expectedFiles)
    {
        var files = directory.GetFiles().Select(x => x.Name).ToList();

        foreach (var expectedFile in expectedFiles)
        {
            if (!files.Contains(expectedFile, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    protected static bool DirectoryContainsFolders(IDirectoryInfo directory, ICollection<string> expectedFolders)
    {
        var folders = directory.GetDirectories();
        if (folders.Length < expectedFolders.Count)
            return false;

        return expectedFolders.All(steamFile =>
            folders.Any(x => x.Name.Equals(steamFile, StringComparison.InvariantCultureIgnoreCase)));
    }
}