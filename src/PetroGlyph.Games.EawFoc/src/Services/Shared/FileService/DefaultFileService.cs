#if NET
using System.Linq;
#else
using System;
#endif
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.FileService;

/// <inheritdoc cref="IPhysicalFileService"/>
public sealed class DefaultFileService : IPhysicalFileService
{
    /// <inheritdoc/>
    public IPhysicalPlayableObject PlayableObject { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="playableObject">The <see cref="IPhysicalPlayableObject"/> this instance is associated to.</param>
    public DefaultFileService(IPhysicalPlayableObject playableObject)
    {
        Requires.NotNull(playableObject, nameof(playableObject));
        PlayableObject = playableObject;
    }

    /// <inheritdoc/>
    public IDirectoryInfo DataDirectory()
    {
        return DataDirectory(string.Empty, true);
    }

    /// <inheritdoc/>
    public IDirectoryInfo DataDirectory(string? subPath, bool checkExists = false)
    {
        var objectPath = PlayableObject.Directory;
        var fs = PlayableObject.FileSystem;
        IDirectoryInfo requestedDirectory;
        subPath ??= string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var dataPath = fs.Path.Combine(objectPath.FullName, "Data", subPath);
            requestedDirectory = fs.DirectoryInfo.New(dataPath);
        }
        else
        {
#if NET
            var searchPattern = fs.Path.Combine("data", subPath);
            requestedDirectory = objectPath.EnumerateDirectories(searchPattern,
                new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).First();
#else
                throw new NotSupportedException();
#endif
        }
        if (checkExists && !requestedDirectory.Exists)
            throw new DirectoryNotFoundException($"Unable to find 'Data' directory of {PlayableObject}");
        return requestedDirectory;
    }

    /// <inheritdoc/>
    public IEnumerable<IFileInfo> DataFiles(
        string fileSearchPattern,
        string? subPath,
        bool throwIfSubPathNotExists,
        bool searchRecursive)
    {
        var searchLocation = DataDirectory(subPath, throwIfSubPathNotExists);
        if (!searchLocation.Exists)
            return new List<IFileInfo>();
#if NET
        return searchLocation.EnumerateFiles(fileSearchPattern,
            new EnumerationOptions
                { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = searchRecursive });
#else
            var options = searchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return searchLocation.EnumerateFiles(fileSearchPattern, options);
#endif
    }
}