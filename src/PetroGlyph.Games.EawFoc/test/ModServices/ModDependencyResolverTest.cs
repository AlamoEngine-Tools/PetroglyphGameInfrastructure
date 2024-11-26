using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyResolverTest : CommonTestBase
{
    //private readonly ModDependencyResolver _resolver;
    //private readonly IGame _game;
    //private readonly IModIdentifierBuilder _identifierBuilder;

    //public ModDependencyResolverTest()
    //{
    //    _resolver = new ModDependencyResolver(ServiceProvider);
    //    _game = CreateRandomGame();
    //    _identifierBuilder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
    //}

    //[Fact]
    //public void InvalidArgs_Throws()
    //{
    //    var mod = CreateRandomGame().InstallAndAddMod("Mod", false, ServiceProvider);
    //    Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(null!));
    //    Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(ServiceProvider).Resolve(mod, null!));
    //    Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(ServiceProvider).Resolve(null!, new DependencyResolverOptions()));
    //}

    //[Fact]
    //public void Resolve_NoDependencies()
    //{
    //    var mod = CreateMod("A");
    //    var dependencies = _resolver.Resolve(mod, new DependencyResolverOptions());
    //    Assert.Empty(dependencies);
    //}

    //[Fact]
    //public void Resolve_DependencyNot_Found()
    //{
    //    var modinfo = new ModinfoData("A")
    //    {
    //        Dependencies = new DependencyList(new List<IModReference>
    //        {
    //            new ModReference("B", ModType.Default)
    //        }, DependencyResolveLayout.FullResolved)
    //    };
    //    var mod = _game.InstallAndAddMod(false, modinfo, ServiceProvider);
    //    Assert.Throws<ModNotFoundException>(() => _resolver.Resolve(mod, new DependencyResolverOptions()));
    //}

    //[Fact]
    //public void Resolve_SelfDependency_WithCycleCheck_Throws()
    //{
    //    var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(_game.ModsLocation.FullName, "A"), ModType.Default));

    //    var modinfo = new ModinfoData("A")
    //    {
    //        Dependencies = new DependencyList(new List<IModReference>
    //        {
    //            depA
    //        }, DependencyResolveLayout.FullResolved)
    //    };
    //    var mod = _game.InstallAndAddMod(false, modinfo, ServiceProvider);
    //    Assert.Throws<ModDependencyCycleException>(() => _resolver.Resolve(mod, new DependencyResolverOptions { CheckForCycle = true }));
    //}

    //[Fact]
    //public void Resolve_SelfDependency_WithoutCycleCheck()
    //{
    //    var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(_game.ModsLocation.FullName, "A"), ModType.Default));

    //    var modinfo = new ModinfoData("A")
    //    {
    //        Dependencies = new DependencyList(new List<IModReference>
    //        {
    //            depA
    //        }, DependencyResolveLayout.FullResolved)
    //    };
    //    var mod = _game.InstallAndAddMod(false, modinfo, ServiceProvider);
    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions { CheckForCycle = false });
    //    Assert.Single(deps);
    //    Assert.Equal(mod, deps[0].Mod);
    //}

    //[Fact]
    //public void Resolve_SingleDependency()
    //{
    //    var dep = CreateMod("B");
    //    var mod = CreateMod("A", DependencyResolveLayout.FullResolved, new ModReference(dep));
        
    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions());

    //    Assert.Single(deps);
    //    Assert.Equal(dep, deps[0].Mod);
    //}

    //[Fact]
    //public void Resolve_TwoDependency()
    //{
    //    var b = CreateMod("B");
    //    var c = CreateMod("C");
    //    var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions());

    //    Assert.Equal([b, c], deps.Select(x => x.Mod));
    //}

    //[Fact]
    //public void Resolve_ResolveCompleteChain_DependenciesHaveDeps_ButResolveLayoutIsFullResolved()
    //{
    //    var b2 = CreateMod("B2");
    //    var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, b2);
    //    var c2 = CreateMod("C2");
    //    var c = CreateMod("C", DependencyResolveLayout.ResolveRecursive, c2);
    //    // Layout is recursive
    //    var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions{ResolveCompleteChain = true});

    //    // Only b and c, because a has layout FullResolved
    //    Assert.Equal([b, c], deps.Select(x => x.Mod));

    //    // ResolveCompleteChain should not have done anything
    //    Assert.Equal(DependencyResolveStatus.None, b.DependencyResolveStatus);
    //    Assert.Equal(DependencyResolveStatus.None, c.DependencyResolveStatus);
    //}

    //[Fact]
    //public void Resolve_ResolveCompleteChain_DependenciesHaveResolvedDeps_ButResolveLayoutIsFullResolved()
    //{
    //    var b2 = CreateMod("B2");
    //    var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, b2);
    //    b.ResolveDependencies(new());
    //    var c2 = CreateMod("C2");
    //    var c = CreateMod("C", DependencyResolveLayout.ResolveRecursive, c2);
    //    c.ResolveDependencies(new());
    //    var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions { ResolveCompleteChain = true });

    //    // Only b and c, because a has layout FullResolved
    //    Assert.Equal([b, c], deps.Select(x => x.Mod));
    //}

    //[Fact]
    //public void Resolve_DependenciesHaveUnresolvedDeps_ResolveLayoutIsRecursive()
    //{
    //    var b2 = CreateMod("B2");
    //    var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, b2);
    //    var c2 = CreateMod("C2");
    //    var c = CreateMod("C", DependencyResolveLayout.ResolveRecursive, c2);
    //    var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b, c);

    //    var deps = _resolver.Resolve(mod, new DependencyResolverOptions { ResolveCompleteChain = false });

    //    Assert.Equal([b, c], deps.Select(x => x.Mod));

    //    Assert.Equal(DependencyResolveStatus.None, b.DependencyResolveStatus);
    //    Assert.Equal(DependencyResolveStatus.None, c.DependencyResolveStatus);
    //}

    //private IMod CreateMod(string name, DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, params IModReference[] deps)
    //{
    //    if (deps.Length == 0)
    //        return _game.InstallAndAddMod(name, GITestUtilities.GetRandomWorkshopFlag(_game), ServiceProvider);

    //    var modinfo = new ModinfoData("A")
    //    {
    //        Dependencies = new DependencyList(deps, layout)
    //    };

    //    return _game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(_game), modinfo, ServiceProvider);
    //}

    //[Fact]
    //public void TestNormalResolveCycle()
    //{
    //    var graph = new Mock<IModDependencyGraph>();
    //    graph.Setup(g => g.HasCycle()).Returns(true);

    //    var graphBuilder = new Mock<IModDependencyGraphBuilder>();
    //    graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>())).Returns(graph.Object);
    //    graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
    //        .Returns(new List<ModDependencyEntry>());

    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);

    //    var game = new Mock<IGame>();
    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.Identifier).Returns("A");
    //    mod.Setup(m => m.Game).Returns(game.Object);

    //    var resolver = new ModDependencyResolver(sp.Object);
    //    var dependencies = resolver.Resolve(mod.Object, new DependencyResolverOptions());

    //    Assert.Empty(dependencies);
    //}

    //[Fact]
    //public void TestNormalResolveCycle_Throws()
    //{
    //    var graph = new Mock<IModDependencyGraph>();
    //    graph.Setup(g => g.HasCycle()).Returns(true);

    //    var graphBuilder = new Mock<IModDependencyGraphBuilder>();
    //    graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>())).Returns(graph.Object);
    //    graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
    //        .Returns(new List<ModDependencyEntry>());

    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);

    //    var game = new Mock<IGame>();
    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.Identifier).Returns("A");
    //    mod.Setup(m => m.Game).Returns(game.Object);

    //    var resolver = new ModDependencyResolver(sp.Object);
    //    Assert.Throws<ModDependencyCycleException>(() =>
    //        resolver.Resolve(mod.Object, new DependencyResolverOptions { CheckForCycle = true }));
    //}

    //[Fact]
    //public void TestNormalResolveChain()
    //{
    //    var game = new Mock<IGame>();

    //    var dep = new Mock<IMod>();
    //    dep.Setup(m => m.Identifier).Returns("Dep");
    //    dep.Setup(m => m.Game).Returns(game.Object);

    //    var graph = new Mock<IModDependencyGraph>();
    //    graph.Setup(g => g.DependenciesOf(It.IsAny<IMod>()))
    //        .Returns(new List<ModDependencyEntry>() { new(dep.Object) });

    //    var graphBuilder = new Mock<IModDependencyGraphBuilder>();
    //    graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
    //        .Returns(new List<ModDependencyEntry>());
    //    graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>()))
    //        .Returns(graph.Object);

    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);


    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.Identifier).Returns("A");
    //    mod.Setup(m => m.Game).Returns(game.Object);

    //    var resolver = new ModDependencyResolver(sp.Object);

    //    var counter = 0;
    //    dep.Setup(
    //            d => d.ResolveDependencies(It.IsAny<IDependencyResolver>(), It.IsAny<DependencyResolverOptions>()))
    //        .Callback<IDependencyResolver, DependencyResolverOptions>((r, o) =>
    //        {
    //            Assert.Equal(resolver, r);
    //            Assert.Equal(new DependencyResolverOptions(), o);
    //            counter++;
    //        });

    //    resolver.Resolve(mod.Object, new DependencyResolverOptions { ResolveCompleteChain = true });
    //    Assert.Equal(1, counter);
    //}
}