using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Registry;

public class TestGameRegistrySetupData
{
    public required GameType GameType { get; init; }

    public bool CreateRegistry { get; set; }

    public bool InitRegistry { get; set; }

    public string? InstallPath { get; set; }

    public int? Revision { get; set; }

    public int? EawGold { get; set; }

    public string? Launcher { get; set; }

    public string? CdKey { get; set; }

    public static TestGameRegistrySetupData Uninitialized(GameType gameType)
    {
        return new TestGameRegistrySetupData
        {
            GameType = gameType,
            CreateRegistry = true,
            InitRegistry = false
        };
    }

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