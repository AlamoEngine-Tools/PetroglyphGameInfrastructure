using System.Collections.Generic;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModEqualityComparerTest : CommonTestBase
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Test_ModsShouldNeverBeEquals(bool includeDeps, bool includeGame)
    {
        var game = FileSystem.InstallGame(new GameIdentity(GameType.Eaw, GamePlatform.Disk), ServiceProvider);

        var modA = game.InstallMod("A", false, ServiceProvider);
        var modB = game.InstallMod("B", false, ServiceProvider);

        var comparer = new ModEqualityComparer(includeDeps, includeGame);
        Assert.False(comparer.Equals(modA, modB));
        Assert.NotEqual(comparer.GetHashCode(modA), comparer.GetHashCode(modB));

        Assert.False(comparer.Equals((IModIdentity)modA, modB));
        Assert.NotEqual(comparer.GetHashCode((IModIdentity)modA), comparer.GetHashCode((IModIdentity)modB));

        Assert.False(comparer.Equals((IModReference)modA, modB));
        Assert.NotEqual(comparer.GetHashCode((IModReference)modA), comparer.GetHashCode((IModReference)modB));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Test_ShouldAlwaysBeEquals(bool includeDeps, bool includeGame)
    {
        var game = FileSystem.InstallGame(new GameIdentity(GameType.Eaw, GamePlatform.Disk), ServiceProvider);

        var modA = game.InstallMod("A", false, ServiceProvider);

        var comparer = new ModEqualityComparer(includeDeps, includeGame);
        Assert.True(comparer.Equals(modA, modA)); 
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modA));

        Assert.True(comparer.Equals((IModIdentity)modA, modA));
        Assert.Equal(comparer.GetHashCode((IModIdentity)modA), comparer.GetHashCode((IModIdentity)modA));

        Assert.True(comparer.Equals(modA, modA));
        Assert.Equal(comparer.GetHashCode((IModReference)modA), comparer.GetHashCode((IModReference)modA));

        var modB = game.InstallMod("A", false, ServiceProvider);
        Assert.True(comparer.Equals(modA, modB));
        Assert.Equal(comparer.GetHashCode(modA), comparer.GetHashCode(modB));

        Assert.True(comparer.Equals((IModIdentity)modA, modB));
        Assert.Equal(comparer.GetHashCode((IModIdentity)modA), comparer.GetHashCode((IModIdentity)modB));

        Assert.True(comparer.Equals((IModReference)modA, modB));
        Assert.Equal(comparer.GetHashCode((IModReference)modA), comparer.GetHashCode((IModReference)modB));
    }

    [Fact]
    public void Test_GameAware()
    {
        var gameA = new Mock<IGame>();
        gameA.Setup(g => g.Equals(null)).Returns(false);
        var comparer = new ModEqualityComparer(false, true);
        Assert.False(comparer.Equals(null, null));
        var modA = new Mock<IMod>();
        modA.Setup(m => m.Identifier).Returns("B");
        modA.Setup(m => m.Game).Returns(gameA.Object);
        Assert.False(comparer.Equals(modA.Object, null));
        Assert.False(comparer.Equals(null, modA.Object));
        Assert.True(comparer.Equals(modA.Object, modA.Object));
        var modB = new Mock<IMod>();
        modB.Setup(m => m.Identifier).Returns("B");
        modB.Setup(m => m.Game).Returns(gameA.Object);
        Assert.False(comparer.Equals(modA.Object, modB.Object));
        gameA.Setup(g => g.Equals(It.IsAny<IGame>())).Returns(true);
        Assert.True(comparer.Equals(modA.Object, modB.Object));
    }

    [Fact]
    public void Test_DependencyAware()
    {
        var comparer = new ModEqualityComparer(true, false);
        Assert.False(comparer.Equals(null, null));
        var modA = new Mock<IMod>();
        Assert.False(comparer.Equals(modA.Object, null));
        Assert.False(comparer.Equals(null, modA.Object));
        Assert.True(comparer.Equals(modA.Object, modA.Object));
        var modB = new Mock<IMod>();
        modA.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry>());
        modB.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry>());
        Assert.True(comparer.Equals(modA.Object, modB.Object));
        modB.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(new Mock<IMod>().Object, null) });
        Assert.False(comparer.Equals(modA.Object, modB.Object));
    }

    [Fact]
    public void Test_ModRef()
    {
        var comparer = new ModEqualityComparer(false, false);
        Assert.False(comparer.Equals(null, null));
        var modA = new Mock<IMod>();
        modA.Setup(m => m.Identifier).Returns("B");
        var modRefA = new Mock<IModReference>();
        var modRefB = new Mock<IModReference>();
        Assert.False(comparer.Equals(modRefA.Object, modA.Object));
        Assert.True(comparer.Equals(modRefA.Object, modRefB.Object));
        modRefA.Setup(r => r.Identifier).Returns("B");
        modRefB.Setup(r => r.Identifier).Returns("A");
        Assert.False(comparer.Equals(modRefA.Object, modRefB.Object));
    }

    [Fact]
    public void Test_ModId()
    {
        var comparer = new ModEqualityComparer(false, false);
        var modidA = new Mock<IModIdentity>();
        var modidB = new Mock<IModIdentity>();
        modidA.Setup(i => i.Name).Returns("N");
        modidB.Setup(i => i.Name).Returns("N");
        Assert.True(comparer.Equals(modidA.Object, modidB.Object));
        modidB.Setup(i => i.Version).Returns(new SemVersion(1, 0, 0));
        Assert.False(comparer.Equals(modidA.Object, modidB.Object));
    }
}