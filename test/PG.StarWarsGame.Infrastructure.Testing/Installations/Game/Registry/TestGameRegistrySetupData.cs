using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Registry;

/// <summary>
/// Represents the setup data for configuring a test game registry.
/// </summary>
public sealed class TestGameRegistrySetupData
{
    /// <summary>
    /// Gets or sets the type of the game.
    /// </summary>
    public required GameType GameType { get; init; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to create the registry.
    /// </summary>
    public bool CreateRegistry { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to initialize the registry.
    /// </summary>
    public bool InitRegistry { get; set; }
   
    /// <summary>
    /// Gets or sets the installation path of the game.
    /// </summary>
    public string? InstallPath { get; set; }
    
    /// <summary>
    /// Gets or sets the revision number of the game.
    /// </summary>
    public int? Revision { get; set; }
    
    /// <summary>
    /// Gets or sets the EAW Gold version identifier.
    /// </summary>
    public int? EawGold { get; set; }
    
    /// <summary>
    /// Gets or sets the path to the game launcher.
    /// </summary>
    public string? Launcher { get; set; }
    
    /// <summary>
    /// Gets or sets the CD key for the game.
    /// </summary>
    public string? CdKey { get; set; }
    
    /// <summary>
    /// Creates an uninitialized setup data instance for the specified game type.
    /// </summary>
    /// <param name="gameType">The type of the game.</param>
    /// <returns>A new instance of <see cref="TestGameRegistrySetupData"/> with default values for uninitialized state.</returns>
    public static TestGameRegistrySetupData Uninitialized(GameType gameType)
    {
        return new TestGameRegistrySetupData
        {
            GameType = gameType,
            CreateRegistry = true,
            InitRegistry = false
        };
    }
    
    /// <summary>
    /// Creates a setup data instance for an installed game.
    /// </summary>
    /// <param name="gameType">The type of the game.</param>
    /// <param name="gameLocation">The directory information of the game installation.</param>
    /// <returns>A new instance of <see cref="TestGameRegistrySetupData"/> configured for an installed game.</returns>
    public static TestGameRegistrySetupData Installed(GameType gameType, IDirectoryInfo gameLocation)
    {
        var revision = gameType == GameType.Eaw ? 10105 : 10100;
        var launcherPath = gameType == GameType.Eaw ? $"{gameLocation.FullName}\\LaunchEAW.exe" : null;
        return new TestGameRegistrySetupData
        {
            GameType = gameType,
            CreateRegistry = true,
            InitRegistry = true,
            Revision = revision,
            EawGold = 20070323,
            CdKey = "%CDKEY%",
            InstallPath = gameLocation.FullName,
            Launcher = launcherPath,
        };
    }
}