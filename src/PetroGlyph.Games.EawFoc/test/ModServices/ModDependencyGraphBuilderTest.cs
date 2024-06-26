﻿using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyGraphBuilderTest
{
    [Fact]
    public void TestNullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModDependencyGraphBuilder().Build(null));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyGraphBuilder().TryBuild(null, out _));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyGraphBuilder().BuildResolveFree(null));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyGraphBuilder().TryBuildResolveFree(null, out _));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyGraphBuilder().GetModDependencyListResolveFree(null));
    }

    [Fact]
    public void TestGetResolveFree_Empty1()
    {
        var mod = new Mock<IMod>();
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.Empty(deps);
    }

    [Fact]
    public void TestGetResolveFree_Empty2()
    {
        var mod = new Mock<IMod>();
        mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry>());
        mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.Empty(deps);
    }

    [Fact]
    public void TestGetResolveFree_Empty3()
    {
        var info = new Mock<IModinfo>();
        info.Setup(i => i.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        var mod = new Mock<IMod>();
        mod.Setup(m => m.ModInfo).Returns(info.Object);
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.Empty(deps);
    }

    [Fact]
    public void TestGetResolveFree_Empty4()
    {
        var depA = new Mock<IMod>();
        depA.Setup(d => d.Identifier).Returns("A");
        var depB = new Mock<IMod>();

        var mod = new Mock<IMod>();
        mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(depA.Object), new(depB.Object) });
        mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Faulted);
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.Empty(deps);
    }

    [Fact]
    public void TestGetResolveFree_Resolved()
    {
        var depA = new Mock<IMod>();
        depA.Setup(d => d.Identifier).Returns("A");
        var depB = new Mock<IMod>();

        var mod = new Mock<IMod>();
        mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(depA.Object), new(depB.Object) });
        mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.NotEmpty(deps);
        Assert.Equal(deps[0].Mod.Identifier, depA.Object.Identifier);
    }

    [Fact]
    public void TestGetResolveFree_FindRef()
    {
        var depA = new Mock<IMod>();
        depA.Setup(d => d.Identifier).Returns("A");

        var game = new Mock<IGame>();
        game.Setup(g => g.FindMod(It.IsAny<IModReference>())).Returns(depA.Object);


        var info = new Mock<IModinfo>();
        info.Setup(i => i.Dependencies)
            .Returns(new DependencyList(new List<IModReference> { depA.Object }, DependencyResolveLayout.FullResolved));
        var mod = new Mock<IMod>();
        mod.Setup(m => m.ModInfo).Returns(info.Object);
        mod.Setup(m => m.Game).Returns(game.Object);
        var deps = new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object);
        Assert.NotEmpty(deps);
        Assert.Equal(deps[0].Mod.Identifier, depA.Object.Identifier);
    }

    [Fact]
    public void TestGetResolveFree_FindRef_Throws()
    {
        var depA = new Mock<IMod>();
        depA.Setup(d => d.Identifier).Returns("A");

        var game = new Mock<IGame>();
        game.Setup(g => g.FindMod(It.IsAny<IModReference>())).Throws(new ModNotFoundException(depA.Object, game.Object));


        var info = new Mock<IModinfo>();
        info.Setup(i => i.Dependencies)
            .Returns(new DependencyList(new List<IModReference> { depA.Object }, DependencyResolveLayout.FullResolved));
        var mod = new Mock<IMod>();
        mod.Setup(m => m.ModInfo).Returns(info.Object);
        mod.Setup(m => m.Game).Returns(game.Object);
        Assert.Throws<ModNotFoundException>(() => new ModDependencyGraphBuilder().GetModDependencyListResolveFree(mod.Object));
    }


    [Fact]
    public void TestBuildNotResolved_Throws()
    {
        var mod = new Mock<IMod>();
        Assert.Throws<ModException>(() => new ModDependencyGraphBuilder().Build(mod.Object));
    }

    [Fact]
    public void TestBuildNotResolvedChain_Throws()
    {
        var depA = new Mock<IMod>();
        depA.Setup(x => x.Equals(It.IsAny<IMod>())).Returns(true);
        depA.Setup(m => m.DependencyResolveLayout).Returns(DependencyResolveLayout.ResolveRecursive);
        depA.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Faulted);

        var mod = new Mock<IMod>();
        mod.Setup(x => x.Equals(It.IsAny<IMod>())).Returns(true);
        mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(depA.Object) });
        mod.Setup(m => m.DependencyResolveLayout).Returns(DependencyResolveLayout.ResolveRecursive);
        mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);

        Assert.Throws<ModException>(() => new ModDependencyGraphBuilder().Build(mod.Object));
    }
}