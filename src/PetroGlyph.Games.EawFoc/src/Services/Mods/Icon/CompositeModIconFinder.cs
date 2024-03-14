using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Icon;

/// <summary>
/// Instance which takes other <see cref="IModIconFinder"/>s and returns the first found icon.
/// </summary>
public class CompositeModIconFinder : IModIconFinder
{
    private readonly IList<IModIconFinder> _orderedFinders;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="orderedFinders">Ordered list of <see cref="IGameIconFinder"/>s.</param>
    public CompositeModIconFinder(IList<IModIconFinder> orderedFinders)
    {
        ThrowHelper.ThrowIfCollectionNullOrEmpty(orderedFinders);
        _orderedFinders = orderedFinders;
    }

    /// <summary>
    /// Searches the internal icon finder instances and returns the first match.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <returns>The first found icon path or <see langword="null"/></returns>
    public string? FindIcon(IMod game)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));

        foreach (var finder in _orderedFinders)
        {
            var iconFile = finder.FindIcon(game);
            if (iconFile is not null)
                return iconFile;
        }
        return null;
    }
}