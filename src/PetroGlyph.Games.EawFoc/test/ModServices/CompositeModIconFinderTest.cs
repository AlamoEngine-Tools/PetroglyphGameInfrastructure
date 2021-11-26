using System;
using System.Collections.Generic;
using Moq;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Icon;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices;

public class CompositeModIconFinderTest
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeModIconFinder(null));
        Assert.Throws<ArgumentException>(() => new CompositeModIconFinder(new List<IModIconFinder>()));
        Assert.Throws<ArgumentNullException>(() =>
            new CompositeModIconFinder(new List<IModIconFinder> { null }).FindIcon(null));
    }

    [Fact]
    public void FindFirst()
    {
        var mod = new Mock<IMod>();
        var a = new Mock<IModIconFinder>();
        a.Setup(f => f.FindIcon(It.IsAny<IMod>())).Returns("path");
        var composite = new CompositeModIconFinder(new List<IModIconFinder> { a.Object });
        Assert.Equal("path", composite.FindIcon(mod.Object));
    }

    [Fact]
    public void FindSecond()
    {
        var mod = new Mock<IMod>();
        var a = new Mock<IModIconFinder>();
        a.Setup(f => f.FindIcon(It.IsAny<IMod>())).Returns((string)null);
        var b = new Mock<IModIconFinder>();
        b.Setup(f => f.FindIcon(It.IsAny<IMod>())).Returns("path");
        var composite = new CompositeModIconFinder(new List<IModIconFinder> { a.Object, b.Object });
        Assert.Equal("path", composite.FindIcon(mod.Object));
    }
}