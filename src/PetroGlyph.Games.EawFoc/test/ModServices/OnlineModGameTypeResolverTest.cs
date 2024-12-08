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
    public override IModGameTypeResolver CreateResolver()
    {
        return new OnlineModGameTypeResolver(ServiceProvider);
    }

    [Theory]
    [InlineData("1125718579", GameType.Foc)] //z3r0x's Mod (3.5)
    [InlineData("2508288191", GameType.Foc, GameType.Eaw)] //Deep Core SDK
    public void TryGetGameType_GetTagsFromSteamOnline(string knownId, params GameType[] expectedTypes)
    {
        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), ServiceProvider);

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var modDir = steamHelpers.GetWorkshopsLocation(game).CreateSubdirectory(knownId);

        // We cannot say for sure to which game a mod belongs if it is in WS directory.
        Assert.True(CreateResolver().TryGetGameType(modDir, ModType.Workshops, null, out var types));
        Assert.Equivalent(expectedTypes, types, true);
    }
}