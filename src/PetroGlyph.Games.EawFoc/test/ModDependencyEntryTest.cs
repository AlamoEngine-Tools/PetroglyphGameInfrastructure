using System;
using Moq;
using PetroGlyph.Games.EawFoc.Mods;
using Semver;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test;

public class ModDependencyEntryTest
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModDependencyEntry(null));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyEntry(null, null));
    }

    [Fact]
    public void TestEquals()
    {
        var modA = new Mock<IMod>();
        modA.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(false);

        var modB = new Mock<IMod>();
        modB.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        var entryA = new ModDependencyEntry(modA.Object);
        var entryB = new ModDependencyEntry(modB.Object);

        var modC = new Mock<IMod>();
        modC.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        var entryC = new ModDependencyEntry(modA.Object, SemVersionRange.Parse("1.*"));

        Assert.Equal(entryA, entryA);
        Assert.NotEqual(entryA, entryB);
        Assert.Equal(entryB, entryC);
    }
}