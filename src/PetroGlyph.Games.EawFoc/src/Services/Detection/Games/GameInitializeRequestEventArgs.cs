using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Event argument for game initialization request.
/// </summary>
public sealed class GameInitializeRequestEventArgs : EventArgs
{
    /// <summary>
    /// Gets the game type that requests initialization.
    /// </summary>
    public GameType GameType { get; }

    /// <summary>
    /// Indicates whether the initialization request was processed. Default is <see langword="false"/>.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Creates a new instances of the <see cref="GameInitializeRequestEventArgs"/> of the specified <see cref="Games.GameType"/>.
    /// </summary>
    /// <param name="gameType">The uninitialized game type.</param>
    public GameInitializeRequestEventArgs(GameType gameType)
    {
        GameType = gameType;
    }
}