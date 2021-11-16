using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

internal class GameExecutableFileService : IGameExecutableFileService
{
    private readonly IServiceProvider _serviceProvider;

    public GameExecutableFileService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileInfo? GetExecutableForGame(IGame game, GameBuildType buildType)
    {
        var nameBuilder = _serviceProvider.GetRequiredService<IGameExecutableNameBuilder>();
        var exeFileName = nameBuilder.GetExecutableFileName(game, buildType);
        if (string.IsNullOrEmpty(exeFileName))
            return null;

#if NETSTANDARD2_1_OR_GREATER
        return game.Directory
            .EnumerateFiles(exeFileName, new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
            .FirstOrDefault();
#else
        return game.Directory
            .EnumerateFiles(exeFileName, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
#endif

    }
}