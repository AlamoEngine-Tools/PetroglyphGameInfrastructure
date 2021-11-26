﻿using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

/// <summary>
/// Get the executable file of a Petroglyph Star Wars game.
/// </summary>
public interface IGameExecutableFileService
{
    /// <summary>
    /// Finds the <paramref name="game"/>'s executable
    /// </summary>
    /// <param name="game"></param>
    /// <param name="buildType"></param>
    /// <returns></returns>
    IFileInfo? GetExecutableForGame(IGame game, GameBuildType buildType);
}