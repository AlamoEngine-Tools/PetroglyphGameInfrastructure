using System.Collections.Generic;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Represents a playable PG entity. Known types are <see cref="IGame"/> and <see cref="IMod"/>
/// </summary>
public interface IPlayableObject
{
    /// <summary>
    /// Gets the name of this playable object.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a set of available languages for this object.
    /// </summary>
    IReadOnlyCollection<ILanguageInfo> InstalledLanguages { get; }

    /// <summary>
    /// Gets the absolute or relative path to an icon of this object.
    /// </summary>
    string? IconFile { get; }

    /// <summary>
    /// Gets the game that is associated to this object. If this object is itself an <see cref="IGame"/> this instance is returned.
    /// </summary>
    IGame Game { get; }
}