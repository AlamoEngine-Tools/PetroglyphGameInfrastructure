﻿using System;
using System.Collections.Generic;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class VirtualModTest
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null, null, null));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null, null, null, DependencyResolveLayout.FullResolved, null));

        var game = new Mock<IGame>();
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(game.Object, null, null));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod("name", null, null, DependencyResolveLayout.FullResolved, null));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(game.Object, new Mock<IModinfo>().Object, null));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod("name", game.Object, new List<ModDependencyEntry>(), DependencyResolveLayout.FullResolved, null));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ModinfoException>(() => new VirtualMod(game.Object, new Mock<IModinfo>().Object, sp.Object));
        Assert.Throws<PetroglyphException>(() => new VirtualMod("name", game.Object, new List<ModDependencyEntry>(), DependencyResolveLayout.FullResolved, sp.Object));
        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(i => i.Name).Returns("Name");
        modinfo.Setup(i => i.Dependencies).Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        Assert.Throws<PetroglyphException>(() => new VirtualMod(game.Object, modinfo.Object, sp.Object));
    }

    [Fact]
    public void ValidCtors_Properties()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Equals(It.IsAny<IGame>())).Returns(true);
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IModIdentifierBuilder))).Returns(new Mock<IModIdentifierBuilder>().Object);
        var dep = new Mock<IMod>();
        dep.Setup(d => d.Game).Returns(game.Object);
        var mod = new VirtualMod("Name", game.Object, new List<ModDependencyEntry> { new(dep.Object) }, DependencyResolveLayout.ResolveLastItem, sp.Object);

        Assert.Single(mod.Dependencies);
        Assert.Equal("Name", mod.Name);
        Assert.Equal(DependencyResolveLayout.ResolveLastItem, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Fact]
    public void NotSupportedOperation_Throws()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Equals(It.IsAny<IGame>())).Returns(true);
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IModIdentifierBuilder))).Returns(new Mock<IModIdentifierBuilder>().Object);
        var dep = new Mock<IMod>();
        dep.Setup(d => d.Game).Returns(game.Object);
        var mod = new VirtualMod("Name", game.Object, new List<ModDependencyEntry> { new(dep.Object) }, DependencyResolveLayout.ResolveLastItem, sp.Object);
        Assert.Throws<NotSupportedException>(() => mod.ResolveDependencies(null, null));
    }
}