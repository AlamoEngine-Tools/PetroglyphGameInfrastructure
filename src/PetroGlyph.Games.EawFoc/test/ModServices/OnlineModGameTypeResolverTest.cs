using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class OnlineModGameTypeResolverTest : ModGameTypeResolverTestBase
{
    public override ModGameTypeResolver CreateResolver()
    {
        return new OnlineModGameTypeResolver(ServiceProvider);
    }

    public static IEnumerable<object[]> GetOnlineModsData()
    {
        yield return ["1125718579", new[] {GameType.Foc}, GameType.Eaw]; //z3r0x's Mod (3.5)
        yield return ["2508288191", new[] {GameType.Foc, GameType.Eaw}, null!]; //Deep Core SDK
    }

    [Theory]
    [MemberData(nameof(GetOnlineModsData))]
    public void Online_GetTagsFromSteamOnline(string knownId, ICollection<GameType> expectedTypes, GameType? incompatibleWith)
    {
        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), ServiceProvider);

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory(knownId);

        var info = CreateDetectedModReference(modDir, ModType.Workshops, null);
        var resolver = CreateResolver();

        // We cannot say for sure to which game a mod belongs if it is in WS directory.
        Assert.True(CreateResolver().TryGetGameType(info, out var types));
        Assert.Equivalent(expectedTypes, types, true);

        Assert.False(resolver.IsDefinitelyNotCompatibleToGame(info, TestHelpers.GetRandom(expectedTypes)));
        if (incompatibleWith is not null)
            Assert.True(resolver.IsDefinitelyNotCompatibleToGame(info, incompatibleWith.Value));
    }
}