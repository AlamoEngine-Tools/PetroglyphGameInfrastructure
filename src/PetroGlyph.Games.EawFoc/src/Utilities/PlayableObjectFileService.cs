using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Utilities;

internal sealed class PlayableObjectFileService(IServiceProvider serviceProvider) : IPlayableObjectFileService
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public IDirectoryInfo DataDirectory(IPhysicalPlayableObject playableObject, string? subPath, bool checkExists = false)
    {
        if (playableObject == null)
            throw new ArgumentNullException(nameof(playableObject));

        var objectPath = playableObject.Directory;
        IDirectoryInfo requestedDirectory;
        subPath ??= string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var dataPath = _fileSystem.Path.Combine(objectPath.FullName, "Data", subPath);
            requestedDirectory = _fileSystem.DirectoryInfo.New(dataPath);
        }
        else
        {
#if NET || NETSTANDARD2_1_OR_GREATER
            var searchPattern = _fileSystem.Path.Combine("data", subPath);
            requestedDirectory = objectPath.EnumerateDirectories(searchPattern,
                new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).First();
#else
            throw new NotSupportedException();
#endif
        }
        if (checkExists && !requestedDirectory.Exists)
            throw new DirectoryNotFoundException($"Unable to find 'Data' directory of {playableObject}");
        return requestedDirectory;
    }

    public IEnumerable<IFileInfo> DataFiles(
        IPhysicalPlayableObject playableObject,
        string fileSearchPattern,
        string? subPath,
        bool throwIfSubPathNotExists,
        bool searchRecursive)
    {
        var searchLocation = DataDirectory(playableObject, subPath, throwIfSubPathNotExists);
        if (!searchLocation.Exists)
            return new List<IFileInfo>();
#if NET || NETSTANDARD2_1_OR_GREATER
        return searchLocation.EnumerateFiles(fileSearchPattern,
            new EnumerationOptions
            { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = searchRecursive });
#else
        var options = searchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return searchLocation.EnumerateFiles(fileSearchPattern, options);
#endif
    }
}