using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModEqualityComparerTest : CommonTestBaseWithRandomGame
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Equals_ModsShouldNeverBeEquals(bool includeDeps, bool includeGame)
    {
        var modA = Game.InstallMod("A", false, ServiceProvider);
        var modB = Game.InstallMod("B", false, ServiceProvider);

        var comparer = new ModEqualityComparer(includeDeps, includeGame);

        Assert.False(comparer.Equals(modA, null));
        Assert.False(comparer.Equals(null, modA));
        Assert.True(comparer.Equals(null, null));

        Assert.False(comparer.Equals(modA, modB));
        Assert.NotEqual(comparer.GetHashCode(modA), comparer.GetHashCode(modB));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Equals_ShouldAlwaysBeEquals(bool includeDeps, bool includeGame)
    {
        var modA = Game.InstallMod("A", false, ServiceProvider);

        var comparer = new ModEqualityComparer(includeDeps, includeGame);
        Assert.True(comparer.Equals(modA, modA));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modA));

        var samish = Game.InstallMod("A", false, ServiceProvider);
        Assert.True(comparer.Equals(modA, samish));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(samish));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equals_DependencyAware(bool depAware)
    {
        var dep = new ModReference("B", ModType.Default);
        var modA = (IPhysicalMod)CreateAndAddMod("A", deps: dep);

        var sameishModindo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
        };
        var modSamish = new Mod(Game, modA.Identifier, modA.Directory, modA.Type == ModType.Workshops, sameishModindo,
            ServiceProvider);

        var differentDep = new Mod(Game, modA.Identifier, modA.Directory, modA.Type == ModType.Workshops, "A",
            ServiceProvider);

        var comparer = new ModEqualityComparer(depAware, TestHelpers.RandomBool());
        Assert.True(comparer.Equals(modA, modA));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modA));

        Assert.True(comparer.Equals(modA, modSamish));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modSamish));

        if (depAware)
        {
            Assert.False(comparer.Equals(modA, differentDep));
            Assert.NotEqual(comparer.GetHashCode(modA), comparer.GetHashCode(differentDep));
        }
        else
        {
            Assert.True(comparer.Equals(modA, differentDep));
            Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(differentDep));
        }
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equals_GameAware(bool gameAware)
    { 
        var modA = CreateAndAddMod("A");

        var sameishGame = new PetroglyphStarWarsGame(Game, Game.Directory, Game.Name, ServiceProvider);
        var modSamish = sameishGame.InstallAndAddMod(modA.Name, modA.Type == ModType.Workshops, ServiceProvider);

        var diffGame = FileSystem.InstallGame(
                new GameIdentity(Game.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw,
                    TestHelpers.GetRandom(GITestUtilities.RealPlatforms)), ServiceProvider);

        var diffGameMod = diffGame.InstallMod("A", modA.Type == ModType.Workshops, ServiceProvider);

        var comparer = new ModEqualityComparer(TestHelpers.RandomBool(), gameAware);
        Assert.True(comparer.Equals(modA, modA));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modA));

        Assert.True(comparer.Equals(modA, modSamish));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modSamish));

        if (gameAware)
        {
            Assert.False(comparer.Equals(modA, diffGameMod));
            Assert.NotEqual(comparer.GetHashCode(modA), comparer.GetHashCode(diffGameMod));
        }
        else
        {
            Assert.True(comparer.Equals(modA, diffGameMod));
            Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(diffGameMod));
        }
    }

    [Fact]
    public void ToJson()
    {
        var mod = CreateAndAddMod("A");
        var expected = new ModReference(mod).ToJson();
        Assert.Equal(expected, mod.ToJson());
    }
}