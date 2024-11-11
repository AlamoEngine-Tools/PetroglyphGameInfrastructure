using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
#if NET || NETSTANDARD2_1_OR_GREATER
using System.Linq;
using System.Runtime.InteropServices;
#endif

namespace PG.StarWarsGame.Infrastructure.Utilities;

/// <summary>
/// Provides extension methods to the <see cref="IPhysicalPlayableObject"/> class to search for directories and files inside the object's Data directory.
/// </summary>
public static class PlayableObjectExtensions
{
    /// <summary>
    /// Returns an <see cref="IDirectoryInfo"/> of the Data directory and the optional sub path for the specified physical playable object.
    /// </summary>
    /// <remarks>
    /// This method does not ensure the specified directory exists. Use <see cref="IFileSystemInfo.Exists"/> to get that information.
    /// <br/>
    /// This method does not check whether <paramref name="subPath"/> leaves the objects directory
    /// by e.g, using absolute paths or path operators ("..").
    /// </remarks>
    /// <param name="playableObject">The playable object the get the requested directory info for.</param>
    /// <param name="subPath">
    /// An optional path of subdirectories of the object's Data directory.
    /// <see langword="null"/> if no subdirectories are requested.
    /// </param>
    /// <returns>The <see cref="IDirectoryInfo"/> of the object's Data directory or the specified subdirectory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public static IDirectoryInfo DataDirectory(this IPhysicalPlayableObject playableObject, string? subPath = null)
    {
        if (playableObject == null)
            throw new ArgumentNullException(nameof(playableObject));

        var objectPath = playableObject.Directory;
        subPath ??= string.Empty;

        var fs = playableObject.Directory.FileSystem;

#if NET || NETSTANDARD2_1_OR_GREATER
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var searchPattern = fs.Path.Combine("Data", subPath);
            var candidateDir = objectPath.EnumerateDirectories(searchPattern,
                new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).FirstOrDefault();
            if (candidateDir is not null)
                return candidateDir;
        }
#endif

        var fullPath = fs.Path.Combine(playableObject.Directory.FullName, "Data", subPath);
        return fs.DirectoryInfo.New(fullPath);
    }

    /// <summary>
    /// Returns an enumerable collection of file information inside the object's data directory or specified subdirectory
    /// that matches a specified search pattern and search recursive option.
    /// If the specified directory does not exist an empty collection is returned.
    /// </summary>
    /// <remarks>
    /// This method does not check whether <paramref name="subPath"/> leaves the objects directory
    /// by e.g, using absolute paths or path operators ("..").
    /// </remarks>
    /// <param name="playableObject">The playable object to search the file information for.</param>
    /// <param name="fileSearchPattern">
    /// The search string to match against the names of files.
    /// This parameter can contain a combination of valid literal path and wildcard (* and ?) characters,
    /// but it doesn't support regular expressions.
    /// </param>
    /// <param name="subPath">
    /// An optional path of subdirectories of the object's Data directory.
    /// <see langword="null"/> if no subdirectories are requested.
    /// </param>
    /// <param name="searchRecursive">Specifies whether the search operation should include only the requested directory or all subdirectories.</param>
    /// <returns>An enumerable collection of files inside the object's data directory or subdirectory that matches <paramref name="searchRecursive"/> and <paramref name="searchRecursive"/>.</returns>
    public static IEnumerable<IFileInfo> DataFiles(
        this IPhysicalPlayableObject playableObject,
        string fileSearchPattern,
        string? subPath,
        bool searchRecursive)
    {
        if (playableObject == null) 
            throw new ArgumentNullException(nameof(playableObject));
        if (fileSearchPattern == null) 
            throw new ArgumentNullException(nameof(fileSearchPattern));

        var searchLocation = DataDirectory(playableObject, subPath);
        if (!searchLocation.Exists)
            return [];

#if NET || NETSTANDARD2_1_OR_GREATER
        return searchLocation.EnumerateFiles(fileSearchPattern, new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive, 
            RecurseSubdirectories = searchRecursive
        });
#else
        var options = searchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return searchLocation.EnumerateFiles(fileSearchPattern, options);
#endif
    }
}