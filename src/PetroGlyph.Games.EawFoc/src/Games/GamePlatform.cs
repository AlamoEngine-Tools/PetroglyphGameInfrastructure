namespace PetroGlyph.Games.EawFoc.Games;

/// <summary>
/// The platform the game was packaged for.
/// </summary>
public enum GamePlatform
{
    /// <summary>
    /// Has a special purpose which can vary between different services which make use of this enum.
    /// </summary>
    Undefined,
    /// <summary>
    /// Vanilla disk/DVD release
    /// </summary>
    Disk,
    /// <summary>
    /// Disk Gold edition where EaW and FoC are on the same disk
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