﻿namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Represents the build configuration of a game.
/// </summary>
public enum GameBuildType
{
    /// <summary>
    /// Release version of the game. That's the type that you get when purchasing the game.
    /// </summary>
    Release,
    /// <summary>
    /// Debug version of the game. Enables logging and other features, useful for developers.
    /// </summary>
    Debug
}