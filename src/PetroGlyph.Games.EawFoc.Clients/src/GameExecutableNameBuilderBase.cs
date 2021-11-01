using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

internal abstract class GameExecutableNameBuilderBase : IGameExecutableNameBuilder
{
    protected readonly IFileSystem FileSystem;

    public abstract IReadOnlyCollection<GamePlatform> SupportedPlatforms { get; }

    protected GameExecutableNameBuilderBase(IServiceProvider serviceProvider)
    {
        FileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
    }

    public string GetExecutableFileName(IGame game, GameBuildType buildType)
    {
        return game.Type switch
        {
            GameType.EaW => GetEawExecutableFileName(buildType),
            GameType.Foc => GetFocExecutableFileName(buildType),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected abstract string GetEawExecutableFileName(GameBuildType buildType);

    protected abstract string GetFocExecutableFileName(GameBuildType buildType);
}