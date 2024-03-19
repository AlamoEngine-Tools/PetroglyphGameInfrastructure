using System;

namespace AET.SteamAbstraction.Games;

/// <summary>
/// Indicates the State of a Steam game.
/// </summary>
[Flags]
public enum SteamAppState
{
    /// <summary>
    /// The current game's state is invalid
    /// </summary>
    StateInvalid = 0,
    /// <summary>
    /// The game is not installed
    /// </summary>
    StateUninstalled     = 1,
    /// <summary>
    /// The game requires an update.
    /// </summary>
    StateUpdateRequired  = 2,
    /// <summary>
    /// The game is fully installed.
    /// </summary>
    StateFullyInstalled  = 4,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateEncrypted       = 8,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateLocked = 16,
    /// <summary>
    /// The game installation is missing files.
    /// </summary>
    StateFilesMissing = 32,
    /// <summary>
    /// The game is currently running.
    /// </summary>
    StateAppRunning = 64,
    /// <summary>
    /// The game installation is corrupted.
    /// </summary>
    StateFilesCorrupt = 128,
    /// <summary>
    /// The game is currently being updated.
    /// </summary>
    StateUpdateRunning = 256,
    /// <summary>
    /// The update was paused.
    /// </summary>
    StateUpdatePaused    = 512,
    /// <summary>
    /// The update is scheduled.
    /// </summary>
    StateUpdateStarted   = 1024,
    /// <summary>
    /// The game is getting removed.
    /// </summary>
    StateUninstalling    = 2048,
    /// <summary>
    /// Steam is making a backup of the game.
    /// </summary>
    StateBackupRunning   = 4096,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateReconfiguring = 65536,
    /// <summary>
    /// Integrity check is running.
    /// </summary>
    StateValidating = 131072,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateAddingFiles = 262144,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StatePreallocating = 524288,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateDownloading = 1048576,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateStaging = 2097152,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateCommitting = 4194304,
    /// <summary>
    /// UNKNOWN
    /// </summary>
    StateUpdateStopping = 8388608
}