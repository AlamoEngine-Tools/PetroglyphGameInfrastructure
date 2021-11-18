using System;
using System.Collections.Generic;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Icon;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices;

public class CompositeGameIconFinderTest
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeGameIconFinder(null));
        Assert.Throws<ArgumentException>(() => new CompositeGameIconFinder(new List<IGameIconFinder>()));
        Assert.Throws<ArgumentNullException>(() =>
            new CompositeGameIconFinder(new List<IGameIconFinder> { null }).FindIcon(null));
    }

    [Fact]
    public void FindFirst()
    {
        var game = new Mock<IGame>();
        var a = new Mock<IGameIconFinder>();
        a.Setup(f => f.FindIcon(It.IsAny<IGame>())).Returns("path");
        var composite = new CompositeGameIconFinder(new List<IGameIconFinder> { a.Object });
        Assert.Equal("path", composite.FindIcon(game.Object));
    }

    [Fact]
    public void FindSecond()
    {
        var game = new Mock<IGame>();
        var a = new Mock<IGameIconFinder>();
        a.Setup(f => f.FindIcon(It.IsAny<IGame>())).Returns((string)null);
        var b = new Mock<IGameIconFinder>();
        b.Setup(f => f.FindIcon(It.IsAny<IGame>())).Returns("path");
        var composite = new CompositeGameIconFinder(new List<IGameIconFinder> { a.Object, b.Object });
        Assert.Equal("path", composite.FindIcon(game.Object));
    }
}