namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Provides constant values for Steam installation paths used in testing scenarios.
/// </summary>
public static class TestingSteamConstants
{
    // Ensure starts on path 'steam'. See GameInstallation.cs
    /// <summary>
    /// The root directory name for testing Steam installations.
    /// </summary>
    public const string SteamInstallPath = "steam";

    /// <summary>
    /// The path to the testing Steam executable.
    /// </summary>
    public const string SteamExePath = "steam/steam.exe";
}