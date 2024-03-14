﻿using System;
using System.Globalization;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Name;

/// <summary>
/// English name resolver for games <see cref="IGameNameResolver"/>.
/// </summary>
public class EnglishGameNameResolver : IGameNameResolver
{
    /// <summary>
    /// Returns the english name of the game.
    /// </summary>
    public string ResolveName(IGameIdentity game)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));
        var gameName = game.Type == GameType.EaW
            ? PetroglyphStarWarsGameConstants.EmpireAtWarEnglishNameShort
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionEnglishNameShort;
        var platform = game.Platform.ToString();
        return $"{gameName} ({platform})";
    }

    /// <summary>
    /// Returns the english name of the game, regardless of the given <paramref name="culture"/>
    /// </summary>
    public string ResolveName(IGameIdentity game, CultureInfo culture)
    {
        return ResolveName(game);
    }
}