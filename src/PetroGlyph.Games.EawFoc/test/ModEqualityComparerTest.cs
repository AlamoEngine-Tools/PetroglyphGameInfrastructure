using System.Collections.Generic;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using SemanticVersioning;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test;

public class ModEqualityComparerTest
{
    [Fact]
    public void Test_Minimal()
    {
        var gameA = new Mock<IGame>();
        gameA.Setup(g => g.Equals(It.IsAny<IGame>())).Returns(true);
        var comparer = new ModEqualityComparer(false, false);
        Assert.False(comparer.Equals(null, null));
        var modA = new Mock<IMod>();
        modA.Setup(m => m.Game).Returns(gameA.Object);
        Assert.False(comparer.Equals(modA.Object, null));
        Assert.False(comparer.Equals(null, modA.Object));
        Assert.True(comparer.Equals(modA.Object, modA.Object));
        var modB = new Mock<IMod>();
        Assert.True(comparer.Equals(modA.Object, modB.Object));
        modB.Setup(m => m.Identifier).Returns("B");
        Assert.False(comparer.Equals(modA.Object, modB.Object));
        modA.Setup(m => m.Identifier).Returns("B");
        Assert.True(comparer.Equals(modA.Object, modB.Object));
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
        modidB.Setup(i => i.Version).Returns(new Version(1, 0, 0));
        Assert.False(comparer.Equals(modidA.Object, modidB.Object));
    }
}