using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Testing.Extensions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModEqualityComparerTest : GameInfrastructureTestBaseWithRandomGame
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void GetHashCode_NullArgs_Throws(bool includeDeps, bool includeGame)
    {
        var comparer = new ModEqualityComparer(includeDeps, includeGame);
        Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(null!));
    }


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
        var modA = (IPhysicalMod)InstallAndAddMod("A", deps: dep).Mod;

        var sameishModindo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
        };
        var modSamish = new Mod(Game, modA.Identifier, modA.Directory, modA.Type == ModType.Workshops, sameishModindo,
            ServiceProvider);

        var differentDep = new Mod(Game, modA.Identifier, modA.Directory, modA.Type == ModType.Workshops, "A",
            ServiceProvider);

        var comparer = new ModEqualityComparer(depAware, Random.Bool());
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
        var modA = InstallAndAddMod("A").Mod;

        var otherGameInstallRef = GameInfrastructureTesting.Game(Game, ServiceProvider);
        var modSamish = otherGameInstallRef.InstallAndAddMod(modA.Name, modA.Type == ModType.Workshops);

        var diffGame = GameInfrastructureTesting
            .Game(new GameIdentity(Game.Type == GameType.Eaw ? GameType.Foc : GameType.Eaw, Game.Platform), ServiceProvider)
            .Game;

        var diffGameMod = diffGame.InstallMod("A", modA.Type == ModType.Workshops, ServiceProvider);

        var comparer = new ModEqualityComparer(Random.Bool(), gameAware);
        Assert.True(comparer.Equals(modA, modA));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modA));

        Assert.True(comparer.Equals(modA, modSamish.Mod));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modSamish.Mod));

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
        var mod = InstallAndAddMod("A").Mod;
        var expected = new ModReference(mod).ToJson();
        Assert.Equal(expected, mod.ToJson());
    }
}