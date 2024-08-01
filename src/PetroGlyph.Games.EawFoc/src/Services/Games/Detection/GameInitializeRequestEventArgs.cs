using System;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Event argument for game initialization request.
/// </summary>
public sealed class GameInitializeRequestEventArgs : EventArgs
{
    /// <summary>
    /// The original options how the game was searched.
    /// </summary>
    public GameDetectorOptions Options { get; }

    /// <summary>
    /// Indicates whether the initialization request was processed. Default is <see langword="false"/>.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Creates a new instances with a given <see cref="GameDetectorOptions"/>
    /// </summary>
    /// <param name="options">The option of this instance.</param>
    public GameInitializeRequestEventArgs(GameDetectorOptions options)
    {
        Options = options;
    }
}