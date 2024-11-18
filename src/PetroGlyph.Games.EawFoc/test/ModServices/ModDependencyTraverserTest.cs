using System;
using Moq;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyTraverserTest : CommonTestBase
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModDependencyTraverser(null!));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyTraverser(ServiceProvider).Traverse(null!));
    }

    //[Fact]
    //public void TestCycle_Throws()
    //{
    //    var graph = new Mock<IModDependencyGraph>();
    //    graph.Setup(g => g.HasCycle()).Returns(true);
    //    var builder = new Mock<IModDependencyGraphBuilder>();
    //    builder.Setup(b => b.Build(It.IsAny<IMod>())).Returns(graph.Object);
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(builder.Object);
    //    var mod = new Mock<IMod>();
    //    Assert.Throws<ModDependencyCycleException>(() => new ModDependencyTraverser(sp.Object).Traverse(mod.Object));
    //}
}