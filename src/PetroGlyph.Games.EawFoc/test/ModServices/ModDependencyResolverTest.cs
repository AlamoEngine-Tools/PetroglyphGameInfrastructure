using System;
using System.Collections.Generic;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices
{
    public class ModDependencyResolverTest
    {
        [Fact]
        public void InvalidArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(null));
            var sp = new Mock<IServiceProvider>();
            Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(sp.Object).Resolve(null, null));
            Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(sp.Object).Resolve(null, new DependencyResolverOptions()));
            Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(sp.Object).Resolve(new Mock<IMod>().Object, null));
        }

        [Fact]
        public void TestNormalResolve_Empty()
        {
            var graphBuilder = new Mock<IModDependencyGraphBuilder>();
            graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
                .Returns(new List<ModDependencyEntry>());

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);

            var game = new Mock<IGame>();
            var mod = new Mock<IMod>();
            mod.Setup(m => m.Identifier).Returns("A");
            mod.Setup(m => m.Game).Returns(game.Object);

            var resolver = new ModDependencyResolver(sp.Object);
            var dependencies = resolver.Resolve(mod.Object, new DependencyResolverOptions());

            Assert.Empty(dependencies);
        }

        [Fact]
        public void TestNormalResolveCycle()
        {
            var graph = new Mock<IModDependencyGraph>();
            graph.Setup(g => g.HasCycle()).Returns(true);

            var graphBuilder = new Mock<IModDependencyGraphBuilder>();
            graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>())).Returns(graph.Object);
            graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
                .Returns(new List<ModDependencyEntry>());

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);

            var game = new Mock<IGame>();
            var mod = new Mock<IMod>();
            mod.Setup(m => m.Identifier).Returns("A");
            mod.Setup(m => m.Game).Returns(game.Object);

            var resolver = new ModDependencyResolver(sp.Object);
            var dependencies = resolver.Resolve(mod.Object, new DependencyResolverOptions());

            Assert.Empty(dependencies);
        }

        [Fact]
        public void TestNormalResolveCycle_Throws()
        {
            var graph = new Mock<IModDependencyGraph>();
            graph.Setup(g => g.HasCycle()).Returns(true);

            var graphBuilder = new Mock<IModDependencyGraphBuilder>();
            graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>())).Returns(graph.Object);
            graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
                .Returns(new List<ModDependencyEntry>());

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);

            var game = new Mock<IGame>();
            var mod = new Mock<IMod>();
            mod.Setup(m => m.Identifier).Returns("A");
            mod.Setup(m => m.Game).Returns(game.Object);

            var resolver = new ModDependencyResolver(sp.Object);
            Assert.Throws<ModDependencyCycleException>(() =>
                resolver.Resolve(mod.Object, new DependencyResolverOptions { CheckForCycle = true }));
        }

        [Fact]
        public void TestNormalResolveChain()
        {
            var game = new Mock<IGame>();

            var dep = new Mock<IMod>();
            dep.Setup(m => m.Identifier).Returns("Dep");
            dep.Setup(m => m.Game).Returns(game.Object);

            var graph = new Mock<IModDependencyGraph>();
            graph.Setup(g => g.DependenciesOf(It.IsAny<IMod>()))
                .Returns(new List<ModDependencyEntry>() { new(dep.Object) });

            var graphBuilder = new Mock<IModDependencyGraphBuilder>();
            graphBuilder.Setup(b => b.GetModDependencyListResolveFree(It.IsAny<IMod>()))
                .Returns(new List<ModDependencyEntry>());
            graphBuilder.Setup(b => b.BuildResolveFree(It.IsAny<IMod>()))
                .Returns(graph.Object);

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IModDependencyGraphBuilder))).Returns(graphBuilder.Object);


            var mod = new Mock<IMod>();
            mod.Setup(m => m.Identifier).Returns("A");
            mod.Setup(m => m.Game).Returns(game.Object);

            var resolver = new ModDependencyResolver(sp.Object);

            var counter = 0;
            dep.Setup(
                    d => d.ResolveDependencies(It.IsAny<IDependencyResolver>(), It.IsAny<DependencyResolverOptions>()))
                .Callback<IDependencyResolver, DependencyResolverOptions>((r, o) =>
                {
                    Assert.Equal(resolver, r);
                    Assert.Equal(new DependencyResolverOptions(), o);
                    counter++;
                });

            resolver.Resolve(mod.Object, new DependencyResolverOptions { ResolveCompleteChain = true });
            Assert.Equal(1, counter);
        }
    }
}