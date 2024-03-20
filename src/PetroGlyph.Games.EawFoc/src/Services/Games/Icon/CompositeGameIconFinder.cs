using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Instance which takes other <see cref="IGameIconFinder"/>s and returns the first found icon.
/// </summary>
public class CompositeGameIconFinder : IGameIconFinder
{
    private readonly IList<IGameIconFinder> _orderedFinders;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="orderedFinders">Ordered list of <see cref="IGameIconFinder"/>s.</param>
    public CompositeGameIconFinder(IList<IGameIconFinder> orderedFinders)
    {
        ThrowHelper.ThrowIfCollectionNullOrEmpty(orderedFinders);
        _orderedFinders = orderedFinders;
    }

    /// <summary>
    /// Searches the internal icon finder instances and returns the first match.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <returns>The first found icon path or <see langword="null"/></returns>
    public string? FindIcon(IGame game)
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