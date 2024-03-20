using System;
using Moq;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyTraverserTest
{
    // The rest gets directly tested as integration test because i'm too lazy to mock all this twice.

    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModDependencyTraverser(null));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new ModDependencyTraverser(sp.Object).Traverse(null));
    }

    [Fact]
    public void TestCycle_Throws()
    {
        var graph = new Mock<IModDependencyGraph>();
        graph.Setup(g => g.HasCycle()).Returns(true);
        var builder = new Mock<IModDependencyGraphBuilder>();
        builder.Setup(b => b.Build(It.IsAny<IMod>())).Returns(graph.Object);
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(builder.Object);
        var mod = new Mock<IMod>();
        Assert.Throws<ModDependencyCycleException>(() => new ModDependencyTraverser(sp.Object).Traverse(mod.Object));
    }
}