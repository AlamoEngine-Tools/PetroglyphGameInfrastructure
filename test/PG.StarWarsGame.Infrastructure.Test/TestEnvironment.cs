using System;

namespace PG.StarWarsGame.Infrastructure.Test;

public static class TestEnvironment
{
    public const string SteamLiveTestsSkipReason =
        "Live Steam-network test. Set STEAM_LIVE_TESTS=1 to enable.";

    public static bool SteamLiveTestsEnabled =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STEAM_LIVE_TESTS"));
}
