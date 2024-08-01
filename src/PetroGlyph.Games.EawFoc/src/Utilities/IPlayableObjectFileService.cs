using System.Collections.Generic;
using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Utilities;

internal interface IPlayableObjectFileService
{
    IDirectoryInfo DataDirectory(IPhysicalPlayableObject playableObject, string? subPath, bool checkExists = false);

    IEnumerable<IFileInfo> DataFiles(IPhysicalPlayableObject playableObject, string fileSearchPattern, string? subPath, bool throwIfSubPathNotExists, bool searchRecursive);
}