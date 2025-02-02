namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// Represents the platform the game distribution.
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// Has a special purpose which can vary between different services which make use of this value.
    /// </summary>
    Undefined,
    /// <summary>
    /// Initial disk/DVD release, where Empire at War and Forces of Corruption are on two separate disks.
    /// </summary>
    Disk,
    /// <summary>
    /// Disk Gold edition where Empire at War and Forces of Corruption are on the same disk
    /// </summary>
    DiskGold,
    /// <summary>
    /// Steam release of the games.
    /// </summary>
    SteamGold,
    /// <summary>
    /// Good Old Games (GOG) release of the games.
    /// </summary>
    GoG,
    /// <summary>
    /// EA Origin release of the game.
    /// </summary>
    Origin
}