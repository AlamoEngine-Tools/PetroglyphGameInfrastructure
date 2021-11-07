using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

internal class DefaultGameExecutableNameBuilder : GameExecutableNameBuilderBase
{
    public override IReadOnlyCollection<GamePlatform> SupportedPlatforms => new List<GamePlatform>
    {
        GamePlatform.Disk,
        GamePlatform.DiskGold,
        GamePlatform.GoG,
        GamePlatform.Origin
    };

    public DefaultGameExecutableNameBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override string GetEawExecutableFileName(GameBuildType buildType)
    {
        return PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName;
    }

    protected override string GetFocExecutableFileName(GameBuildType buildType)
    {
        return PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;
    }
}