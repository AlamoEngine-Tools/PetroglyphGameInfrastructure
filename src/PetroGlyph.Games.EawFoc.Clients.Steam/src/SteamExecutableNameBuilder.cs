using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal class SteamExecutableNameBuilder : GameExecutableNameBuilderBase
{
    private const string FileNameBase = "StarWars";
    private const string ReleaseSuffix = "G";
    private const string DebugSuffix = "I";

    public override IReadOnlyCollection<GamePlatform> SupportedPlatforms => new List<GamePlatform> { GamePlatform.SteamGold };

    public SteamExecutableNameBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override string GetEawExecutableFileName(GameBuildType buildType)
    {
        return GetFileName(buildType);
    }

    protected override string GetFocExecutableFileName(GameBuildType buildType)
    {
        return GetFileName(buildType);
    }

    private string GetFileName(GameBuildType buildType)
    {
        var suffix = buildType switch
        {
            GameBuildType.Release => ReleaseSuffix,
            GameBuildType.Debug => DebugSuffix,
            _ => throw new ArgumentOutOfRangeException(nameof(buildType), buildType, null)
        };
        return $"{FileNameBase}{suffix}.exe";
    }
}