using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Services.FileService;

/// <summary>
/// Service to query files and directories of an <see cref="IPhysicalPlayableObject"/>
/// </summary>
// TODO: I don't think we need this interface
public interface IPhysicalFileService
{
    /// <summary>
    /// The <see cref="IPhysicalPlayableObject"/> this instance is assigned to.
    /// </summary>
    IPhysicalPlayableObject PlayableObject { get; }

    /// <summary>
    /// Returns the "Data" directory of the <see cref="PlayableObject"/>.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">if the "Data" directory does not exist.</exception>
    public IDirectoryInfo DataDirectory();

    /// <summary>
    /// Returns a directory relative to the "Data" directory.
    /// </summary>
    /// <param name="subPath">relative sub-path from the "Data" directory. <see langword="null"/>if no sub-path is requested.</param>
    /// <param name="checkExists">When set to <see langword="true"/> a check will be performed whether the requested directory exists.</param>
    /// <exception cref="DirectoryNotFoundException">if the requested directory does not exist and <paramref name="checkExists"/>is set to <see langword="true"/>.</exception>
    public IDirectoryInfo DataDirectory(string? subPath, bool checkExists);

    /// <summary>
    /// Searches the a directory beginning relative from the <see cref="PlayableObject"/>'s "Data" directory for files which match the <paramref name="fileSearchPattern"/>.
    /// </summary>
    /// <param name="fileSearchPattern">The search pattern of the requested files.</param>
    /// <param name="subPath">relative sub-path from the "Data" directory. <see langword="null"/>if no sub-path is requested.</param>
    /// <param name="throwIfSubPathNotExists">When set to <see langword="true"/> a check will be performed whether the requested directory exists; Otherwise returns an empty enumerable if the <paramref name="subPath"/> does not exist.</param>
    /// <param name="searchRecursive">When set to <see langword="true"/> sub directories starting from the given <paramref name="subPath"/> will be searched too.</param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException">if the requested <paramref name="subPath"/> does not exist and <paramref name="throwIfSubPathNotExists"/>is set to <see langword="true"/>.</exception>
    public IEnumerable<IFileInfo> DataFiles(
        string fileSearchPattern, string? subPath,
        bool throwIfSubPathNotExists,
        bool searchRecursive);
}