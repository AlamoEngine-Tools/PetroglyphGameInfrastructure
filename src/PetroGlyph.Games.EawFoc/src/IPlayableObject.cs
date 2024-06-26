﻿using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Flag interface to identify an object which can be played in some sort.
/// Known types are <see cref="IGame"/> and <see cref="IMod"/>
/// </summary>
public interface IPlayableObject
{
    /// <summary>
    /// The name of this playable object.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Set of available languages for this instance.
    /// </summary>
    ISet<ILanguageInfo> InstalledLanguages { get; }

    /// <summary>
    /// Absolute or relative path to an icon of this instance.
    /// </summary>
    string? IconFile { get; }

    /// <summary>
    /// The game associated to this instance.
    /// If this instance is an <see cref="IGame"/> this returns itself.
    /// </summary>
    IGame Game { get; }
}